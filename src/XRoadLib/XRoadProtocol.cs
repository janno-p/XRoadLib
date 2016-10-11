using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib
{
    /// <summary>
    /// X-Road message protocol implementation details.
    /// </summary>
    public interface IXRoadProtocol
    {
        /// <summary>
        /// String form of message protocol version.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// XML document style of messages (RPC/Encoded or Document/Literal).
        /// </summary>
        Style Style { get; }

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
        string ProducerNamespace { get; }

        /// <summary>
        /// Header definition of the protocol.
        /// </summary>
        HeaderDefinition HeaderDefinition { get; }

        /// <summary>
        /// Protocol specification.
        /// </summary>
        ProtocolDefinition ProtocolDefinition { get; }

        /// <summary>
        /// Get runtime types lookup object.
        /// </summary>
        ISerializerCache GetSerializerCache(uint? version = null);

        /// <summary>
        /// Generates new service description for current message protocol instance.
        /// </summary>
        void WriteServiceDescription(Stream outputStream, uint? version = null);
    }

    /// <summary>
    /// X-Road message protocol implementation details.
    /// </summary>
    public class XRoadProtocol : IXRoadProtocol
    {
        private readonly IDictionary<uint, ISerializerCache> serializerCaches = new Dictionary<uint, ISerializerCache>();

        private readonly SchemaDefinitionProvider schemaDefinitionProvider;

        /// <summary>
        /// String form of message protocol version.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// XML document style of messages (RPC/Encoded or Document/Literal).
        /// </summary>
        public Style Style => ProtocolDefinition.Style;

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
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
        /// Initializes new X-Road message protocol instance.
        /// <param name="name">Identifies protocol instance.</param>
        /// <param name="schemaExporter">Schema customization provider.</param>
        /// </summary>
        public XRoadProtocol(string name, ISchemaExporter schemaExporter)
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
        /// Generates new service description for current message protocol instance.
        /// </summary>
        public void WriteServiceDescription(Stream outputStream, uint? version = null)
        {
            if (!version.HasValue && ProtocolDefinition.SupportedVersions.Any())
                throw new ArgumentNullException(nameof(version), "Version value is required to generate service description.");

            if (version.HasValue && !ProtocolDefinition.SupportedVersions.Contains(version.Value))
                throw new ArgumentOutOfRangeException(nameof(version), $"Version {version.Value} is not supported.");

            new ProducerDefinition(this, schemaDefinitionProvider, version).SaveTo(outputStream);
        }

        /// <summary>
        /// Get runtime types lookup object.
        /// </summary>
        public ISerializerCache GetSerializerCache(uint? version = null)
        {
            if (!serializerCaches.Any())
                throw new Exception($"This protocol instance (message protocol version `{Name}`) is not configured with contract assembly.");

            if (!ProtocolDefinition.SupportedVersions.Any())
                return serializerCaches.Single().Value;

            if (!version.HasValue)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) requires specific version value.");

            var serializerVersion = version.Value > 0u ? version.Value : ProtocolDefinition.SupportedVersions.Max();

            ISerializerCache versioningSerializerCache;
            if (serializerCaches.TryGetValue(version.Value, out versioningSerializerCache))
                return versioningSerializerCache;

            throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.", nameof(version));
        }

        private void SetContractAssembly()
        {
            if (ProtocolDefinition.ContractAssembly == null)
                throw new Exception($"SchemaExporter must define contract assembly of types and operations.");

            if (!ProtocolDefinition.SupportedVersions.Any())
            {
                serializerCaches.Add(0, new SerializerCache(this, schemaDefinitionProvider));
                return;
            }

            foreach (var dtoVersion in ProtocolDefinition.SupportedVersions)
                serializerCaches.Add(dtoVersion, new SerializerCache(this, schemaDefinitionProvider, dtoVersion));
        }
    }
}