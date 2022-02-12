using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using System.Xml.Schema;
using XRoadLib.Wsdl;

namespace XRoadLib.Styles;

/// <summary>
/// X-Road message serialization style.
/// </summary>
public abstract class Style
{
    /// <summary>
    /// Use this instance to create XmlNodes.
    /// </summary>
    protected readonly XmlDocument Document = new();

    /// <summary>
    /// Writes explicit type attribute if style requires it.
    /// </summary>
    public virtual Task WriteExplicitTypeAsync(XmlWriter writer, XName qualifiedName) =>
        Task.CompletedTask;

    /// <summary>
    /// Writes explicit array type attribute if style requires it.
    /// </summary>
    public virtual Task WriteExplicitArrayTypeAsync(XmlWriter writer, XName itemQualifiedName, int arraySize) =>
        Task.CompletedTask;

    /// <summary>
    /// Writes element type attribute according to style preferences.
    /// </summary>
    [UsedImplicitly]
    public virtual async Task WriteTypeAsync(XmlWriter writer, TypeDefinition typeDefinition, ContentDefinition contentDefinition)
    {
        if (typeDefinition.IsAnonymous)
            return;

        if (typeDefinition.Type != contentDefinition.RuntimeType)
        {
            await writer.WriteTypeAttributeAsync(typeDefinition.Name).ConfigureAwait(false);
            return;
        }

        if (contentDefinition.Particle is not RequestDefinition)
            await WriteExplicitTypeAsync(writer, typeDefinition.Name).ConfigureAwait(false);
    }

    /// <summary>
    /// Serializes X-Road SOAP message header element.
    /// </summary>
    public async Task WriteHeaderElementAsync(XmlWriter writer, XName name, object value, XName typeName)
    {
        await writer.WriteStartElementAsync(null, name.LocalName, name.NamespaceName).ConfigureAwait(false);

        await WriteExplicitTypeAsync(writer, typeName).ConfigureAwait(false);

        if (typeName.LocalName == XmlTypeConstants.String.LocalName && typeName.NamespaceName == NamespaceConstants.Xsd)
            await writer.WriteStringWithModeAsync((string)value ?? "", StringSerializationMode).ConfigureAwait(false);
        else writer.WriteValue(value);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Create operation binding for current style.
    /// </summary>
    public abstract SoapOperationBinding CreateSoapOperationBinding(string soapAction);

    /// <summary>
    /// Create soap body binding for current style.
    /// </summary>
    public abstract SoapBodyBinding CreateSoapBodyBinding(string targetNamespace);

    /// <summary>
    /// Create header binding binding for current style.
    /// </summary>
    public abstract SoapHeaderBinding CreateSoapHeaderBinding(XName headerName, string messageName, string targetNamespace);

    /// <summary>
    /// Add expected content type attribute for binary content.
    /// </summary>
    [UsedImplicitly]
    public virtual XmlAttribute CreateExpectedContentType(string contentType)
    {
        var attribute = Document.CreateAttribute(PrefixConstants.Xmime, "expectedContentTypes", NamespaceConstants.Xmime);
        attribute.Value = contentType;
        return attribute;
    }

    /// <summary>
    /// Create array item element for collection type.
    /// </summary>
    public abstract void AddItemElementToArrayElement(XmlSchemaElement arrayElement, XmlSchemaElement itemElement, Action<string> addSchemaImport);

    /// <summary>
    /// Create soap binding element for current style.
    /// </summary>
    public abstract SoapBinding CreateSoapBinding();

    /// <summary>
    /// Should message definitions use type or element references.
    /// </summary>
    [UsedImplicitly]
    public virtual bool UseElementInMessagePart => true;

    /// <summary>
    /// Preferred string serialization mode.
    /// </summary>
    [UsedImplicitly]
    public virtual StringSerializationMode StringSerializationMode => StringSerializationMode.HtmlEncoded;

    [UsedImplicitly]
    public virtual async Task SerializeFaultAsync(XmlWriter writer, IXRoadFault fault)
    {
        await writer.WriteStartElementAsync("faultCode").ConfigureAwait(false);
        await WriteExplicitTypeAsync(writer, XmlTypeConstants.String).ConfigureAwait(false);
        await writer.WriteStringAsync(fault.FaultCode).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);

        await writer.WriteStartElementAsync("faultString").ConfigureAwait(false);
        await WriteExplicitTypeAsync(writer, XmlTypeConstants.String).ConfigureAwait(false);
        await writer.WriteStringAsync(fault.FaultString).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}