using System.Reflection;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Styles;
using XRoadLib.Wsdl;

namespace XRoadLib.Schema;

/// <summary>
/// Extracts serialization/definition details from runtime types and methods.
/// </summary>
[UsedImplicitly]
public class SchemaDefinitionProvider
{
    private readonly ISchemaExporter _schemaExporter;

    /// <summary>
    /// Global settings for protocol instance.
    /// </summary>
    public ProtocolDefinition ProtocolDefinition { get; }

    /// <summary>
    /// Initializes definition builder.
    /// </summary>
    public SchemaDefinitionProvider(ISchemaExporter schemaExporter)
    {
        _schemaExporter = schemaExporter ?? throw new ArgumentNullException(nameof(schemaExporter));

        ProtocolDefinition = GetProtocolDefinition();
    }

    /// <summary>
    /// Initializes default simple type definition and applies customizations (if any).
    /// </summary>
    public TypeDefinition GetSimpleTypeDefinition<T>(string typeName)
    {
        var isType = !string.IsNullOrEmpty(typeName);

        var typeDefinition = new TypeDefinition(typeof(T), isType ? NamespaceConstants.Xsd : "")
        {
            Name = isType ? XName.Get(typeName, NamespaceConstants.Xsd) : null,
            IsSimpleType = true
        };

        _schemaExporter.ExportTypeDefinition(typeDefinition);

        return typeDefinition;
    }

    /// <summary>
    /// Initializes default collection type definition and applies customizations (if any).
    /// </summary>
    public CollectionDefinition GetCollectionDefinition(TypeDefinition typeDefinition)
    {
        var collectionDefinition = new CollectionDefinition(typeDefinition.Type.MakeArrayType(), typeDefinition, typeDefinition.TargetNamespace)
        {
            CanHoldNullValues = true,
            IsAnonymous = true
        };

        _schemaExporter.ExportTypeDefinition(collectionDefinition);

        return collectionDefinition;
    }

    /// <summary>
    /// Initializes default type definition and applies customizations (if any).
    /// </summary>
    public TypeDefinition GetTypeDefinition(Type type, string? typeName = null)
    {
        XName qualifiedName = null;

        if (type.IsArray)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
                qualifiedName = XName.Get(typeName, ProtocolDefinition.ProducerNamespace);

            var collectionDefinition = new CollectionDefinition(type, GetTypeDefinition(type.GetElementType()!), ProtocolDefinition.ProducerNamespace)
            {
                Name = qualifiedName,
                IsAnonymous = qualifiedName == null,
                CanHoldNullValues = true
            };

            _schemaExporter.ExportTypeDefinition(collectionDefinition);

            return collectionDefinition;
        }

        var typeAttribute = type.GetCustomAttribute<XmlTypeAttribute>();
        var isAnonymous = typeAttribute is { AnonymousType: true };

        var normalizedType = type.NormalizeType();
        var targetNamespace = typeAttribute?.Namespace ?? ProtocolDefinition.ProducerNamespace;

        if (!isAnonymous)
            qualifiedName = XName.Get((typeAttribute?.TypeName).GetValueOrDefault(typeName ?? normalizedType.Name), targetNamespace);

        var typeDefinition = new TypeDefinition(normalizedType, targetNamespace)
        {
            ContentComparer = PropertyComparer.Instance,
            HasStrictContentOrder = true,
            IsAnonymous = isAnonymous,
            Name = qualifiedName,
            State = DefinitionState.Default,
            CanHoldNullValues = type.IsClass || normalizedType != type
        };

        _schemaExporter.ExportTypeDefinition(typeDefinition);

        return typeDefinition;
    }

    /// <summary>
    /// Initializes default property definition and applies customizations (if any).
    /// </summary>
    public PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition)
    {
        var propertyDefinition = new PropertyDefinition(propertyInfo, typeDefinition, _schemaExporter.IsQualifiedElementDefault);

        _schemaExporter.ExportPropertyDefinition(propertyDefinition);

        return propertyDefinition;
    }

    /// <summary>
    /// Initializes default request element definition and applies customizations (if any).
    /// </summary>
    public RequestDefinition GetRequestDefinition(OperationDefinition operationDefinition)
    {
        var requestDefinition = new RequestDefinition(operationDefinition, _schemaExporter.IsQualifiedElementDefault);

        operationDefinition.ExtensionSchemaExporter?.ExportRequestDefinition(requestDefinition);
        _schemaExporter.ExportRequestDefinition(requestDefinition);

        return requestDefinition;
    }

    /// <summary>
    /// Initializes default response element definition and applies customizations (if any).
    /// </summary>
    public ResponseDefinition GetResponseDefinition(OperationDefinition operationDefinition, XRoadFaultPresentation? xRoadFaultPresentation = null)
    {
        var responseDefinition = new ResponseDefinition(operationDefinition, _schemaExporter.IsQualifiedElementDefault)
        {
            XRoadFaultPresentation = xRoadFaultPresentation ?? XRoadFaultPresentation.Choice
        };

        operationDefinition.ExtensionSchemaExporter?.ExportResponseDefinition(responseDefinition);
        _schemaExporter.ExportResponseDefinition(responseDefinition);

        return responseDefinition;
    }

    /// <summary>
    /// Initializes default opeartion definition and applies customizations (if any).
    /// </summary>
    public OperationDefinition GetOperationDefinition(MethodInfo methodInfo, XName qualifiedName, uint? version)
    {
        var operationDefinition = new OperationDefinition(qualifiedName, version, methodInfo);

        operationDefinition.ExtensionSchemaExporter?.ExportOperationDefinition(operationDefinition);
        _schemaExporter.ExportOperationDefinition(operationDefinition);

        return operationDefinition;
    }

    /// <summary>
    /// Initializes default fault definition and applies customizations (if any).
    /// </summary>
    public FaultDefinition GetFaultDefinition()
    {
        var faultDefinition = new FaultDefinition { Name = XName.Get("fault", ProtocolDefinition.ProducerNamespace) };

        _schemaExporter.ExportFaultDefinition(faultDefinition);

        return faultDefinition;
    }

    /// <summary>
    /// Get schema location of specified schema namespace.
    /// </summary>
    public string? GetSchemaLocation(string namespaceName, ISchemaExporter? extension = null)
    {
        return _schemaExporter.ExportSchemaLocation(namespaceName)
               ?? extension?.ExportSchemaLocation(namespaceName)
               ?? NamespaceConstants.GetSchemaLocation(namespaceName);
    }

    /// <summary>
    /// Customize service description before presentation.
    /// </summary>
    public void ExportServiceDescription(ServiceDescription serviceDescription)
    {
        _schemaExporter.ExportServiceDescription(serviceDescription);
    }

    /// <summary>
    /// Get preferred X-Road namespace prefix for service description.
    /// </summary>
    public string GetXRoadPrefix() => _schemaExporter.XRoadPrefix ?? PrefixConstants.XRoad;

    /// <summary>
    /// Get main namespace which defines X-Road message protocol specifics.
    /// </summary>
    public string GetXRoadNamespace() => _schemaExporter.XRoadNamespace ?? NamespaceConstants.XRoad;

    /// <summary>
    /// Customize X-Road message header elements.
    /// </summary>
    public HeaderDefinition GetXRoadHeaderDefinition()
    {
        var headerDefinition = new HeaderDefinition { MessageName = "RequiredHeaders" };

        headerDefinition.Use<XRoadHeader>()
                        .WithRequiredHeader(x => x.Client)
                        .WithRequiredHeader(x => x.Service)
                        .WithRequiredHeader(x => x.UserId)
                        .WithRequiredHeader(x => x.Id)
                        .WithRequiredHeader(x => x.Issue)
                        .WithRequiredHeader(x => x.ProtocolVersion)
                        .WithHeaderNamespace(NamespaceConstants.XRoad)
                        .WithHeaderNamespace(NamespaceConstants.XRoadRepr);

        _schemaExporter.ExportHeaderDefinition(headerDefinition);

        return headerDefinition;
    }

    /// <summary>
    /// Returns `true` if given namespace defines qualified element names by default.
    /// </summary>
    public virtual bool IsQualifiedElementDefault(string namespaceName) =>
        _schemaExporter.IsQualifiedElementDefault(namespaceName);

    private ProtocolDefinition GetProtocolDefinition()
    {
        var protocolDefinition = new ProtocolDefinition { Style = new DocLiteralStyle() };

        _schemaExporter.ExportProtocolDefinition(protocolDefinition);

        if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.SoapEnv))
            protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.SoapEnv), PrefixConstants.SoapEnv);

        if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.Xsd))
            protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.Xsd), PrefixConstants.Xsd);

        if (!protocolDefinition.GlobalNamespacePrefixes.ContainsKey(NamespaceConstants.Xsi))
            protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.Xsi), PrefixConstants.Xsi);

        if (!string.IsNullOrEmpty(protocolDefinition.ProducerNamespace) && !protocolDefinition.GlobalNamespacePrefixes.ContainsKey(protocolDefinition.ProducerNamespace))
            protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(protocolDefinition.ProducerNamespace), PrefixConstants.Target);

        return protocolDefinition;
    }
}