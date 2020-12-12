using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization;
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
        public IHeaderDefinition HeaderDefinition { get; }

        /// <inheritdoc />
        public ProtocolDefinition ProtocolDefinition { get; }

        /// <inheritdoc />
        public XRoadMessage CreateMessage() =>
            new XRoadMessage(this, HeaderDefinition.CreateHeader());

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
    }
}