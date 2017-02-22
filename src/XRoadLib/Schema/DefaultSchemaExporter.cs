using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Headers;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Default schema exporter of X-Road message protocol version 4.0.
    /// Generates schema description and serialization logic according to
    /// default settings.
    /// </summary>
    public class DefaultSchemaExporter : AbstractSchemaExporter
    {
        private readonly Assembly contractAssembly;

        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public override string XRoadPrefix => PrefixConstants.XROAD;

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public override string XRoadNamespace => NamespaceConstants.XROAD_V4;

        /// <summary>
        /// Defines list of supported DTO versions (for DTO based versioning).
        /// </summary>
        public ISet<uint> SupportedVersions { get; } = new HashSet<uint>();

        /// <summary>
        /// Define list of content filters of X-Road message elements.
        /// </summary>
        public ISet<string> EnabledFilters { get; } = new HashSet<string>();

        /// <summary>
        /// Initializes new schema exporter instance and configure minimal set
        /// of configuration options.
        /// </summary>
        public DefaultSchemaExporter(string producerNamespace, Assembly contractAssembly)
            : base(producerNamespace)
        {
            this.contractAssembly = contractAssembly;
        }

        /// <summary>
        /// Configure SOAP header of the messages.
        /// </summary>
        public override void ExportHeaderDefinition(HeaderDefinition headerDefinition)
        {
            base.ExportHeaderDefinition(headerDefinition);

            headerDefinition.Use<XRoadHeader40>()
                            .WithRequiredHeader(x => x.Client)
                            .WithRequiredHeader(x => x.Service)
                            .WithRequiredHeader(x => x.UserId)
                            .WithRequiredHeader(x => x.Id)
                            .WithRequiredHeader(x => x.ProtocolVersion)
                            .WithHeaderNamespace(NamespaceConstants.XROAD_V4);
        }

        /// <summary>
        /// Configure protocol global settings.
        /// </summary>
        public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
        {
            base.ExportProtocolDefinition(protocolDefinition);

            protocolDefinition.ContractAssembly = contractAssembly;

            foreach (var version in SupportedVersions)
                protocolDefinition.SupportedVersions.Add(version);

            foreach (var filter in EnabledFilters)
                protocolDefinition.EnabledFilters.Add(filter);
        }
    }
}