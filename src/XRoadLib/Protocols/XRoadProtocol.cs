using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using XRoadLib.Protocols.Description;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;
using XRoadLib.Serialization;

#if NET40
using XRoadLib.Extensions;
#endif

namespace XRoadLib.Protocols
{
    /// <summary>
    /// X-Road message protocol implementation details.
    /// </summary>
    public class XRoadProtocol
    {
        private readonly SchemaDefinitionReader schemaDefinitionReader;
        private readonly ProtocolDefinition protocolDefinition;
        private readonly HeaderDefinition headerDefinition;

        private IDictionary<uint, ISerializerCache> versioningSerializerCaches;
        private ISerializerCache serializerCache;

        /// <summary>
        /// String form of message protocol version.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Global versions supported by this X-Road message protocol instance.
        /// </summary>
        public IEnumerable<uint> SupportedVersions => versioningSerializerCaches?.Keys ?? Enumerable.Empty<uint>();

        /// <summary>
        /// XML document style of messages (RPC/Encoded or Document/Literal).
        /// </summary>
        public Style Style => protocolDefinition.Style;

        /// <summary>
        /// Main namespace which defines current producer operations and types.
        /// </summary>
        public string ProducerNamespace => protocolDefinition.ProducerNamespace;

        /// <summary>
        /// Assembly which provides runtime types for operations and types.
        /// </summary>
        public Assembly ContractAssembly { get; private set; }

        /// <summary>
        /// Initialize new X-Road message header.
        /// </summary>
        public Func<IXRoadHeader> CreateHeader => headerDefinition.Initializer;

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

            schemaDefinitionReader = new SchemaDefinitionReader(schemaExporter);

            headerDefinition = schemaDefinitionReader.GetXRoadHeaderDefinition();
            protocolDefinition = schemaDefinitionReader.ProtocolDefinition;
        }

        /// <summary>
        /// Initializes new X-Road message protocol instance.
        /// <param name="name">Identifies protocol instance.</param>
        /// <param name="producerNamespace">Producer namespace of this protocol instance.</param>
        /// </summary>
        public XRoadProtocol(string name, string producerNamespace)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(producerNamespace))
                throw new ArgumentNullException(nameof(producerNamespace));

            schemaDefinitionReader = new SchemaDefinitionReader(producerNamespace);

            headerDefinition = schemaDefinitionReader.GetXRoadHeaderDefinition();
            protocolDefinition = schemaDefinitionReader.ProtocolDefinition;
        }

        /// <summary>
        /// Generates new service description for current message protocol instance.
        /// </summary>
        public void WriteServiceDescription(Stream outputStream, uint? version = null)
        {
            new ProducerDefinition(this, schemaDefinitionReader, ContractAssembly, version).SaveTo(outputStream);
        }

        /// <summary>
        /// Associate runtime types with current message protocol instance.
        /// </summary>
        public void SetContractAssemblyOfType<TAssembly>()
        {
            SetContractAssembly(typeof(TAssembly).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Associate runtime types with current message protocol instance.
        /// </summary>
        public void SetContractAssembly(Assembly assembly, params uint[] supportedVersions)
        {
            SetContractAssembly(assembly, null, supportedVersions);
        }

        /// <summary>
        /// Associate runtime types with current message protocol instance.
        /// </summary>
        public void SetContractAssembly(Assembly assembly, IList<string> availableFilters, params uint[] supportedVersions)
        {
            if (ContractAssembly != null)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) already has contract configured.");

            ContractAssembly = assembly;

            if (supportedVersions == null || supportedVersions.Length == 0)
            {
                serializerCache = new SerializerCache(this, schemaDefinitionReader, assembly) { AvailableFilters = availableFilters };
                return;
            }

            versioningSerializerCaches = new Dictionary<uint, ISerializerCache>();
            foreach (var dtoVersion in supportedVersions)
                versioningSerializerCaches.Add(dtoVersion, new SerializerCache(this, schemaDefinitionReader, assembly, dtoVersion) { AvailableFilters = availableFilters });
        }

        /// <summary>
        /// Get runtime types lookup object.
        /// </summary>
        public ISerializerCache GetSerializerCache(uint? version = null)
        {
            if (serializerCache == null && versioningSerializerCaches == null)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) is not configured with contract assembly.");

            if (serializerCache != null)
                return serializerCache;

            if (!version.HasValue)
                throw new Exception($"This protocol instance (message protocol version `{Name}`) requires specific version value.");

            ISerializerCache versioningSerializerCache;
            if (versioningSerializerCaches.TryGetValue(version.Value, out versioningSerializerCache))
                return versioningSerializerCache;

            throw new ArgumentException($"This protocol instance (message protocol version `{Name}`) does not support `v{version.Value}`.", nameof(version));
        }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        public bool IsHeaderNamespace(string namespaceName) => headerDefinition.IsHeaderNamespace(namespaceName);

        /// <summary>
        /// Check if envelope defines given protocol schema.
        /// </summary>
        public bool IsDefinedByEnvelope(XmlReader reader) => protocolDefinition.DetectEnvelope?.Invoke(reader) ?? false;

        /// <summary>
        /// Initialize new X-Road message of this X-Road message protocol instance.
        /// </summary>
        public XRoadMessage NewMessage(IXRoadHeader header = null)
        {
            return new XRoadMessage(this, header ?? CreateHeader());
        }
    }
}