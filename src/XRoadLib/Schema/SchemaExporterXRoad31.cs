using System.Web.Services.Description;
using XRoadLib.Protocols.Headers;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol version 3.1.
    /// </summary>
    public class SchemaExporterXRoad31 : SchemaExporterXRoadLegacy
    {
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public override string XRoadPrefix => PrefixConstants.XROAD;

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public override string XRoadNamespace => NamespaceConstants.XROAD;

        /// <summary>
        /// X-Road standard compliant producer namespace.
        /// </summary>
        public override string ProducerNamespace { get; }

        /// <summary>
        /// Initializes schema exporter for X-Road message protocol version 3.1.
        /// </summary>
        public SchemaExporterXRoad31(string producerName)
            : base(producerName)
        {
            ProducerNamespace = $"http://{producerName}.x-road.ee/producer/";
        }

        /// <summary>
        /// Allows each message protocol implementation to customize service description document
        /// before publishing.
        /// </summary>
        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);
        }

        /// <summary>
        /// Configure SOAP header of the messages.
        /// </summary>
        public override void ExportHeaderDefinition(HeaderDefinition headerDefinition)
        {
            base.ExportHeaderDefinition(headerDefinition);

            headerDefinition.Use<XRoadHeader31>(() => new XRoadHeader31())
                            .WithRequiredHeader(x => x.Consumer)
                            .WithRequiredHeader(x => x.Producer)
                            .WithRequiredHeader(x => ((IXRoadHeader31)x).Service)
                            .WithRequiredHeader(x => x.UserId)
                            .WithRequiredHeader(x => x.Id)
                            .WithRequiredHeader(x => x.UserName);
        }
    }
}