using XRoadLib.Serialization;

namespace XRoadLib.Extensions;

/// <summary>
/// Extension methods for XmlReader class.
/// </summary>
public static class XmlReaderExtensions
{
    private static readonly XName QnXsiNil = XName.Get("nil", NamespaceConstants.Xsi);
    private static readonly XName QnXsiType = XName.Get("type", NamespaceConstants.Xsi);
    private static readonly XName QnSoapEncArray = XName.Get("Array", NamespaceConstants.SoapEnc);
    private static readonly XName QnSoapEncArrayType = XName.Get("arrayType", NamespaceConstants.SoapEnc);

    /// <summary>
    /// Test if current element is marked as nil with xsi attribute.
    /// </summary>
    public static bool IsNilElement(this XmlReader reader) =>
        reader.GetAttribute(QnXsiNil.LocalName, QnXsiNil.NamespaceName) switch
        {
            "1" => true,
            "true" => true,

            "0" => false,
            "false" => false,

            null => false,

            var value => throw new InvalidQueryException($"Invalid {QnXsiNil} attribute value: `{value}`")
        };

    internal static XName GetTypeAttributeValue(this XmlReader reader)
    {
        return GetTypeAttributeValue(reader, QnXsiType);
    }

    private static XName GetTypeAttributeValue(XmlReader reader, XName attributeName, bool isArrayType = false)
    {
        var typeValue = reader.GetAttribute(attributeName.LocalName, attributeName.NamespaceName);
        return ParseQualifiedValue(reader, typeValue, isArrayType);
    }

    internal static XName ParseQualifiedValue(this XmlReader reader, string value, bool isArrayType = false)
    {
        if (value == null)
            return null;

        var namespaceSeparatorIndex = value.IndexOf(':');
        var namespacePrefix = namespaceSeparatorIndex < 0 ? string.Empty : value.Substring(0, namespaceSeparatorIndex);
        var typeName = namespaceSeparatorIndex < 0 ? value : value.Substring(namespaceSeparatorIndex + 1);

        var typeNamespace = reader.LookupNamespace(namespacePrefix);
        if (typeNamespace == null)
            throw new InvalidQueryException($"Undefined namespace prefix `{namespacePrefix}` given in XML message for element `{reader.LocalName}` xsi:type.");

        if (isArrayType)
            typeName = typeName.Substring(0, typeName.LastIndexOf('['));

        var qualifiedName = XName.Get(typeName, typeNamespace);

        return qualifiedName != QnSoapEncArray ? qualifiedName : GetTypeAttributeValue(reader, QnSoapEncArrayType, true);
    }

    /// <summary>
    /// Reposition XML reader to matching end element of the current element.
    /// </summary>
    public static async Task ReadToEndElementAsync(this XmlReader reader)
    {
        if (reader.IsEmptyElement)
            return;

        var currentDepth = reader.Depth;

        while (await reader.ReadAsync().ConfigureAwait(false) && currentDepth < reader.Depth)
        {
            // Do nothing
        }
    }

    /// <summary>
    /// Reposition XML reader to the next element if it's currently at nil element.
    /// </summary>
    public static async Task ConsumeNilElementAsync(this XmlReader reader, bool isNil)
    {
        if (!isNil)
            return;

        if (reader.IsEmptyElement)
        {
            await reader.ReadAsync().ConfigureAwait(false);
            return;
        }

        var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(content))
            throw new InvalidQueryException($@"An element labeled with `xsi:nil=""true""` must be empty, but had `{content}` as content.");

        await reader.ReadToEndElementAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Reposition reader at location where next Read() call will navigate to next node.
    /// </summary>
    public static Task ConsumeUnusedElementAsync(this XmlReader reader)
    {
        return reader.IsEmptyElement ? reader.ReadAsync() : reader.ReadToEndElementAsync();
    }

    /// <summary>
    /// Check if XML reader is currently positioned at the specified element.
    /// </summary>
    public static bool IsCurrentElement(this XmlReader reader, int depth, XName name)
    {
        return reader.NodeType == XmlNodeType.Element && reader.Depth == depth && reader.GetXName() == name;
    }

    /// <summary>
    /// Move XML reader current position to next element which matches the given arguments.
    /// </summary>
    public static async Task<bool> MoveToElementAsync(this XmlReader reader, int depth, XName name = null)
    {
        while (true)
        {
            if (reader.Depth == depth && reader.NodeType == XmlNodeType.Element && (name == null || reader.IsCurrentElement(depth, name)))
                return true;

            if (!await reader.ReadAsync().ConfigureAwait(false) || reader.Depth < depth)
                return false;
        }
    }

    /// <summary>
    /// Get current reader node name as XName.
    /// </summary>
    public static XName GetXName(this XmlReader reader)
    {
        return XName.Get(reader.LocalName, reader.NamespaceURI);
    }

    /// <summary>
    /// Deserialize current node as XRoadFault entity.
    /// </summary>
    public static async Task<IXRoadFault> ReadXRoadFaultAsync(this XmlReader reader, int depth)
    {
        var fault = new XRoadFault();

        while (await reader.ReadAsync().ConfigureAwait(false) && await reader.MoveToElementAsync(depth).ConfigureAwait(false))
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultCode")
                fault.FaultCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultString")
                fault.FaultString = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        }

        return fault;
    }

    internal static async Task<object> MoveNextAndReturnAsync(this XmlReader reader, object value)
    {
        await reader.ReadAsync().ConfigureAwait(false);
        return value;
    }

    internal static async Task<bool> ReadToContentAsync(this XmlReader reader)
    {
        var depth = reader.Depth;
        var childDepth = depth + 1;

        while (true)
        {
            if (reader.Depth == childDepth && (reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA))
                return true;

            if (!await reader.ReadAsync().ConfigureAwait(false) || reader.Depth < childDepth)
                return false;
        }
    }
}