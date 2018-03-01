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
using XRoadLib.Styles;
using XRoadLib.Wsdl;

namespace XRoadLib
{
    /// <summary>
    /// Manages available services and provides their definitions and serialization details.
    /// </summary>
    public class ServiceManager<THeader> : IServiceManager
        where THeader : class, IXRoadHeader, IXRoadHeader<THeader>, new()
    {
        private readonly IDictionary<uint, ISerializer> serializers = new Dictionary<uint, ISerializer>();
        private readonly SchemaDefinitionProvider schemaDefinitionProvider;

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

        /// <inheritdoc />
        public IXRoadHeader ConvertHeader(XRoadCommonHeader commonHeader) =>
            new THeader().InitFrom(commonHeader);

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

            schemaDefinitionProvider = new SchemaDefinitionProvider(schemaExporter);

            HeaderDefinition = schemaDefinitionProvider.GetXRoadHeaderDefinition();
            ProtocolDefinition = schemaDefinitionProvider.ProtocolDefinition;

            SetContractAssembly();
        }

        /// <inheritdoc />
        public virtual TResult Execute<TResult>(WebRequest webRequest, object body, XRoadCommonHeader commonHeader, ServiceExecutionOptions options = null) =>
            Execute<TResult>(webRequest, body, new THeader().InitFrom(commonHeader), options);

        /// <summary>
        /// Executes X-Road operation on endpoint specified by WebRequest parameter.
        /// </summary>
        /// <param name="webRequest">WebRequest used to transfer X-Road messages.</param>
        /// <param name="body">Soap body part of outgoing serialized X-Road message.</param>
        /// <param name="header">Soap header part of outgoing serialized X-Road message.</param>
        /// <param name="options">Additional options to configure service call execution.</param>
        /// <typeparam name="TResult">Expected result type of the operation.</typeparam>
        /// <returns>Deserialized value of X-Road response message Soap body.</returns>
        public virtual TResult Execute<TResult>(WebRequest webRequest, object body, THeader header, ServiceExecutionOptions options = null)
        {
            using (var requestMessage = new XRoadMessage(this, header))
            {
                IServiceMap operationServiceMap;
                using (var writer = XmlWriter.Create(requestMessage.ContentStream))
                {
                    writer.WriteStartDocument();

                    Style.WriteSoapEnvelope(writer, ProducerNamespace);
                    if (!string.IsNullOrEmpty(options?.RequestNamespace))
                        writer.WriteAttributeString(PrefixConstants.XMLNS, "req", NamespaceConstants.XMLNS, options.RequestNamespace);

                    Style.WriteSoapHeader(writer, header, HeaderDefinition);

                    writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);

                    var operationName = XName.Get(options?.OperationName ?? header.Service.ServiceCode, ProducerNamespace);
                    operationServiceMap = options?.ServiceMap ?? GetSerializer(options?.Version ?? requestMessage.Version).GetServiceMap(operationName);
                    operationServiceMap.SerializeRequest(writer, body, requestMessage, options?.RequestNamespace);

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }

                options?.BeforeRequest?.Invoke(this, new XRoadRequestEventArgs(requestMessage));

                requestMessage.SaveTo(webRequest);

                using (var response = webRequest.GetResponseAsync().Result)
                using (var responseStream = response.GetResponseStream())
                using (var seekableStream = new MemoryStream())
                using (var responseMessage = new XRoadMessage())
                {
                    responseStream?.CopyTo(seekableStream);
                    options?.BeforeDeserialize?.Invoke(this, new XRoadResponseEventArgs(response, seekableStream));
                    responseMessage.LoadResponse(seekableStream, response.Headers.GetContentTypeHeader(), Path.GetTempPath(), this);
                    return (TResult)responseMessage.DeserializeMessageContent(operationServiceMap);
                }
            }
        }

        /// <inheritdoc />
        public virtual ServiceDescription CreateServiceDescription(Func<OperationDefinition, bool> operationFilter = null, uint? version = null)
        {
            if (!version.HasValue && ProtocolDefinition.SupportedVersions.Any())
                throw new ArgumentNullException(nameof(version), "Version value is required to generate service description.");

            if (version.HasValue && !ProtocolDefinition.SupportedVersions.Contains(version.Value))
                throw new ArgumentOutOfRangeException(nameof(version), $"Version {version.Value} is not supported.");

            var producerDefinition = new ServiceDescriptionBuilder(schemaDefinitionProvider, operationFilter, version);

            return producerDefinition.GetServiceDescription();
        }

        /// <summary>
        /// Initialize new X-Road message of this X-Road message protocol instance.
        /// </summary>
        public XRoadMessage CreateMessage(THeader header = null)
        {
            return new XRoadMessage(this, header ?? new THeader());
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
            if (!serializers.Any())
                throw new Exception($"This protocol instance (message protocol version `{Name}`) is not configured with contract assembly.");

            if (!ProtocolDefinition.SupportedVersions.Any())
                return serializers.Single().Value;

            if (!version.HasValue)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) requires specific version value.");

            var serializerVersion = version.Value > 0u ? version.Value : ProtocolDefinition.SupportedVersions.Max();

            if (serializers.TryGetValue(serializerVersion, out var versioningSerializer))
                return versioningSerializer;

            throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.", nameof(version));
        }

        private void SetContractAssembly()
        {
            if (ProtocolDefinition.ContractAssembly == null)
                throw new Exception("SchemaExporter must define contract assembly of types and operations.");

            if (!ProtocolDefinition.SupportedVersions.Any())
            {
                serializers.Add(0, new Serializer(schemaDefinitionProvider));
                return;
            }

            foreach (var dtoVersion in ProtocolDefinition.SupportedVersions)
                serializers.Add(dtoVersion, new Serializer(schemaDefinitionProvider, dtoVersion));
        }

        IXRoadHeader IServiceManager.CreateHeader() => new THeader();
    }
}