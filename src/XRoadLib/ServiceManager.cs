using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib
{
    public class ServiceManager : IServiceManager
    {
        private readonly IDictionary<uint, ISerializer> _serializers = new Dictionary<uint, ISerializer>();
        private readonly ISchemaProvider _schemaProvider;

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
            new(this, HeaderDefinition.CreateHeader());

        /// <summary>
        /// Initializes new X-Road service manager instance.
        /// <param name="name">Identifies service manager instance.</param>
        /// <param name="schemaProvider">Schema customization provider.</param>
        /// </summary>
        public ServiceManager(string name, ISchemaProvider schemaProvider)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));

            Name = name;

            HeaderDefinition = _schemaProvider.GetXRoadHeaderDefinition();
            ProtocolDefinition = _schemaProvider.GetProtocolDefinition();

            SetContractAssembly();
        }

        /// <inheritdoc />
        public virtual async Task WriteServiceDefinitionAsync(XmlWriter writer, Func<OperationDefinition, bool> operationFilter = null, uint? version = null)
        {
            if (!version.HasValue && ProtocolDefinition.SupportedVersions.Any())
                throw new SchemaDefinitionException("Version value is required to generate service description.");

            if (version.HasValue && !ProtocolDefinition.SupportedVersions.Contains(version.Value))
                throw new SchemaDefinitionException($"Version {version.Value} is not supported.");

            var producerDefinition = new ServiceDescriptionBuilder(_schemaProvider, operationFilter, version);

            await writer.WriteStartDocumentAsync().ConfigureAwait(false);
            await producerDefinition.GetServiceDescription().WriteAsync(writer).ConfigureAwait(false);
            await writer.WriteEndDocumentAsync().ConfigureAwait(false);

            await writer.FlushAsync().ConfigureAwait(false);
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
                _serializers.Add(0, new Serializer(_schemaProvider));
                return;
            }

            foreach (var dtoVersion in ProtocolDefinition.SupportedVersions)
                _serializers.Add(dtoVersion, new Serializer(_schemaProvider, dtoVersion));
        }
    }
}