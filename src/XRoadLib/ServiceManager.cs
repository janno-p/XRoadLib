using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Styles;

namespace XRoadLib
{
    public interface IServiceManager
    {
        string Name { get; }
        
        Style Style { get; }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        bool IsHeaderNamespace(string namespaceName);

        /// <summary>
        /// Check if envelope defines given protocol schema.
        /// </summary>
        bool IsDefinedByEnvelope(XmlReader reader);

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
        string ProducerNamespace { get; }

        ISerializer GetSerializer(uint? version = null);

        IXRoadHeader CreateHeader();

        ServiceDescription CreateServiceDescription(uint? version = null);
    }
    
    /// <summary>
    /// Manages available services and provides their definitions and serialization details.
    /// </summary>
    public class ServiceManager<THeader> : IServiceManager
        where THeader : class, IXRoadHeader, new()
    {
        private readonly IDictionary<uint, ISerializer> serializers = new Dictionary<uint, ISerializer>();
        private readonly SchemaDefinitionProvider schemaDefinitionProvider;
        
        /// <summary>
        /// Used to uniquely identify service manager instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// XML document style of messages (RPC/Encoded or Document/Literal).
        /// </summary>
        public Style Style => ProtocolDefinition.Style;

        /// <inheritdoc />
        public string ProducerNamespace => ProtocolDefinition.ProducerNamespace;

        /// <summary>
        /// Header definition of the protocol.
        /// </summary>
        public HeaderDefinition HeaderDefinition { get; }

        /// <summary>
        /// Protocol specification.
        /// </summary>
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

            schemaDefinitionProvider = new SchemaDefinitionProvider(schemaExporter);

            HeaderDefinition = schemaDefinitionProvider.GetXRoadHeaderDefinition();
            ProtocolDefinition = schemaDefinitionProvider.ProtocolDefinition;

            SetContractAssembly();
        }

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

                    operationServiceMap = options?.ServiceMap ?? requestMessage.GetSerializer().GetServiceMap(XName.Get(header.Service.ServiceCode, ProducerNamespace));
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

        /// <summary>
        /// Generates new service description for specified services.
        /// </summary>
        public virtual ServiceDescription CreateServiceDescription(uint? version = null)
        {
            if (!version.HasValue && ProtocolDefinition.SupportedVersions.Any())
                throw new ArgumentNullException(nameof(version), "Version value is required to generate service description.");

            if (version.HasValue && !ProtocolDefinition.SupportedVersions.Contains(version.Value))
                throw new ArgumentOutOfRangeException(nameof(version), $"Version {version.Value} is not supported.");

            var producerDefinition = new ProducerDefinition(schemaDefinitionProvider, version);

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

        /// <summary>
        /// Get runtime types lookup object.
        /// </summary>
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