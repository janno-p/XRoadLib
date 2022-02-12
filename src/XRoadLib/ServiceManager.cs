using System.Net;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;
using XRoadLib.Styles;
using XRoadLib.Wsdl;

namespace XRoadLib;

public class ServiceManager : IServiceManager
{
    private readonly IDictionary<uint, ISerializer> _serializers = new Dictionary<uint, ISerializer>();
    private readonly SchemaDefinitionProvider _schemaDefinitionProvider;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Style Style => ProtocolDefinition.Style;

    /// <inheritdoc />
    public string ProducerNamespace => ProtocolDefinition.ProducerNamespace;

    /// <inheritdoc />
    public HeaderDefinition HeaderDefinition { get; }

    /// <inheritdoc />
    public ProtocolDefinition ProtocolDefinition { get; }

    /// <summary>
    /// Initializes new X-Road service manager instance.
    /// <param name="name">Identifies service manager instance.</param>
    /// <param name="schemaExporter">Schema customization provider.</param>
    /// </summary>
    [UsedImplicitly]
    public ServiceManager(string name, ISchemaExporter schemaExporter)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (schemaExporter == null)
            throw new ArgumentNullException(nameof(schemaExporter));

        Name = name;

        _schemaDefinitionProvider = new SchemaDefinitionProvider(schemaExporter);

        HeaderDefinition = _schemaDefinitionProvider.GetXRoadHeaderDefinition();
        ProtocolDefinition = _schemaDefinitionProvider.ProtocolDefinition;

        SetContractAssembly();
    }

    /// <inheritdoc />
    async Task<object> IServiceManager.ExecuteAsync(WebRequest webRequest, object body, ISoapHeader header, ServiceExecutionOptions options)
    {
        var messageFormatter = options?.MessageFormatter ?? new SoapMessageFormatter();

        using var requestMessage = new XRoadMessage(this, header);

        IServiceMap operationServiceMap;
        using (var writer = XmlWriter.Create(requestMessage.ContentStream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
        {
            await writer.WriteStartDocumentAsync().ConfigureAwait(false);

            await writer.WriteSoapEnvelopeAsync(messageFormatter, ProtocolDefinition).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(options?.RequestNamespace) && writer.LookupPrefix(options.RequestNamespace) == null)
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, "req", NamespaceConstants.Xmlns, options.RequestNamespace).ConfigureAwait(false);

            await messageFormatter.WriteSoapHeaderAsync(writer, Style, header, HeaderDefinition).ConfigureAwait(false);
            await messageFormatter.WriteStartBodyAsync(writer).ConfigureAwait(false);

            var serviceCode = (header as IXRoadHeader)?.Service?.ServiceCode ?? string.Empty;

            var operationName = XName.Get(options?.OperationName ?? serviceCode, ProducerNamespace);
            operationServiceMap = options?.ServiceMap ?? GetSerializer(options?.Version ?? requestMessage.Version).GetServiceMap(operationName);
            await operationServiceMap.SerializeRequestAsync(writer, body, requestMessage, options?.RequestNamespace).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndDocumentAsync().ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }

        options?.BeforeRequest?.Invoke(this, new XRoadRequestEventArgs(requestMessage));

        await requestMessage.SaveToAsync(webRequest, messageFormatter).ConfigureAwait(false);

        using var response = await webRequest.GetResponseAsync().ConfigureAwait(false);
        using var responseStream = response.GetResponseStream();
        using var seekableStream = new MemoryStream();
        using var responseMessage = new XRoadMessage();

        if (responseStream != null)
            await responseStream.CopyToAsync(seekableStream).ConfigureAwait(false);

        options?.BeforeDeserialize?.Invoke(this, new XRoadResponseEventArgs(response, seekableStream));
        await responseMessage.LoadResponseAsync(seekableStream, messageFormatter, response.Headers.GetContentTypeHeader(), Path.GetTempPath(), this).ConfigureAwait(false);

        return await responseMessage.DeserializeMessageContentAsync(operationServiceMap, messageFormatter).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual ServiceDescription CreateServiceDescription(Func<OperationDefinition, bool> operationFilter = null, uint? version = null)
    {
        if (!version.HasValue && ProtocolDefinition.SupportedVersions.Any())
            throw new SchemaDefinitionException("Version value is required to generate service description.");

        if (version.HasValue && !ProtocolDefinition.SupportedVersions.Contains(version.Value))
            throw new SchemaDefinitionException($"Version {version.Value} is not supported.");

        var producerDefinition = new ServiceDescriptionBuilder(_schemaDefinitionProvider, operationFilter, version);

        return producerDefinition.GetServiceDescription();
    }

    /// <inheritdoc />
    public bool IsHeaderNamespace(string namespaceName) =>
        HeaderDefinition.IsHeaderNamespace(namespaceName);

    /// <inheritdoc />
    public async Task<bool> IsDefinedByEnvelopeAsync(XmlReader reader)
    {
        if (ProtocolDefinition.DetectEnvelopeAsync != null)
            return await ProtocolDefinition.DetectEnvelopeAsync(reader).ConfigureAwait(false);

        return false;
    }

    /// <inheritdoc />
    public virtual ISerializer GetSerializer(uint? version = null)
    {
        if (!_serializers.Any())
            throw new SchemaDefinitionException($"This protocol instance (message protocol version `{Name}`) is not configured with contract assembly.");

        if (!ProtocolDefinition.SupportedVersions.Any())
            return _serializers.Single().Value;

        if (!version.HasValue)
            throw new SchemaDefinitionException($"This protocol instance (message protocol version `{Name}`) requires specific version value.");

        var serializerVersion = version.Value > 0u ? version.Value : ProtocolDefinition.SupportedVersions.Max();

        if (_serializers.TryGetValue(serializerVersion, out var versionSerializer))
            return versionSerializer;

        throw new SchemaDefinitionException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.");
    }

    private void SetContractAssembly()
    {
        if (ProtocolDefinition.ContractAssembly == null)
            throw new SchemaDefinitionException("SchemaExporter must define contract assembly of types and operations.");

        if (!ProtocolDefinition.SupportedVersions.Any())
        {
            _serializers.Add(0, new Serializer(_schemaDefinitionProvider));
            return;
        }

        foreach (var dtoVersion in ProtocolDefinition.SupportedVersions)
            _serializers.Add(dtoVersion, new Serializer(_schemaDefinitionProvider, dtoVersion));
    }

    protected virtual ISoapHeader CreateHeader() =>
        null;

    ISoapHeader IServiceManager.CreateHeader() =>
        CreateHeader();
}