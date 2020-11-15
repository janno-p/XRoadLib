using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;
using XRoadLib.Styles;
using XRoadLib.Wsdl;

namespace XRoadLib
{
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
        object IServiceManager.Execute(WebRequest webRequest, object body, ISoapHeader header, ServiceExecutionOptions options)
        {
            var messageFormatter = options?.MessageFormatter ?? new SoapMessageFormatter();

            using (var requestMessage = new XRoadMessage(this, header))
            {
                IServiceMap operationServiceMap;
                using (var writer = XmlWriter.Create(requestMessage.ContentStream))
                {
                    writer.WriteStartDocument();

                    writer.WriteSoapEnvelope(messageFormatter, ProtocolDefinition);
                    if (!string.IsNullOrEmpty(options?.RequestNamespace) && writer.LookupPrefix(options.RequestNamespace) == null)
                        writer.WriteAttributeString(PrefixConstants.Xmlns, "req", NamespaceConstants.Xmlns, options.RequestNamespace);

                    messageFormatter.WriteSoapHeader(writer, Style, header, HeaderDefinition);
                    messageFormatter.WriteStartBody(writer);

                    var serviceCode = (header as IXRoadHeader)?.Service?.ServiceCode ?? string.Empty;

                    var operationName = XName.Get(options?.OperationName ?? serviceCode, ProducerNamespace);
                    operationServiceMap = options?.ServiceMap ?? GetSerializer(options?.Version ?? requestMessage.Version).GetServiceMap(operationName);
                    operationServiceMap.SerializeRequest(writer, body, requestMessage, options?.RequestNamespace);

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }

                options?.BeforeRequest?.Invoke(this, new XRoadRequestEventArgs(requestMessage));

                requestMessage.SaveTo(webRequest, messageFormatter);

                using (var response = webRequest.GetResponseAsync().Result)
                using (var responseStream = response.GetResponseStream())
                using (var seekableStream = new MemoryStream())
                using (var responseMessage = new XRoadMessage())
                {
                    responseStream?.CopyTo(seekableStream);
                    options?.BeforeDeserialize?.Invoke(this, new XRoadResponseEventArgs(response, seekableStream));
                    responseMessage.LoadResponse(seekableStream, messageFormatter, response.Headers.GetContentTypeHeader(), Path.GetTempPath(), this);
                    return responseMessage.DeserializeMessageContent(operationServiceMap, messageFormatter);
                }
            }
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
        public bool IsDefinedByEnvelope(XmlReader reader) =>
            ProtocolDefinition.DetectEnvelope?.Invoke(reader) ?? false;

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

    /// <summary>
    /// Manages available services and provides their definitions and serialization details.
    /// </summary>
    public class ServiceManager<THeader> : ServiceManager
        where THeader : class, ISoapHeader, new()
    {
        /// <inheritdoc cref="ServiceManager" />
        public ServiceManager(string name, ISchemaExporter schemaExporter)
            : base(name, schemaExporter)
        { }

        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="webRequest">WebRequest used to transfer X-Road messages.</param>
        /// <param name="body">Soap body part of outgoing serialized X-Road message.</param>
        /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <typeparam name="TResult">Expected result type of the operation.</typeparam>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        public virtual TResult Execute<TResult>(WebRequest webRequest, object body, THeader header, ServiceExecutionOptions options = null) =>
            (TResult)((IServiceManager)this).Execute(webRequest, body, header, options);

        /// <summary>
        /// Initialize new X-Road message of this X-Road message protocol instance.
        /// </summary>
        public XRoadMessage CreateMessage(THeader header = null)
        {
            return new XRoadMessage(this, header ?? new THeader());
        }

        protected override ISoapHeader CreateHeader() =>
            new THeader();
    }
}