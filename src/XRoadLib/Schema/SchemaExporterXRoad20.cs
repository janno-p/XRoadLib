using System.Web.Services.Description;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol version 2.0.
    /// </summary>
    public class SchemaExporterXRoad20 : SchemaExporterXRoadLegacy
    {
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public override string XRoadPrefix => PrefixConstants.XTEE;

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public override string XRoadNamespace => NamespaceConstants.XTEE;

        /// <summary>
        /// X-Road standard compliant producer namespace.
        /// </summary>
        public override string ProducerNamespace { get; }

        /// <summary>
        /// Initializes schema exporter for X-Road message protocol version 2.0.
        /// </summary>
        public SchemaExporterXRoad20(string producerName)
            : base(producerName)
        {
            ProducerNamespace = $"http://producers.{producerName}.xtee.riik.ee/producer/{producerName}";
        }

        /// <summary>
        /// Configure request elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportRequestValueDefinition(RequestValueDefinition requestValueDefinition)
        {
            base.ExportRequestValueDefinition(requestValueDefinition);

            requestValueDefinition.RequestElementName = "keha";
        }

        /// <summary>
        /// Configure response elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition)
        {
            base.ExportResponseValueDefinition(responseValueDefinition);

            responseValueDefinition.RequestElementName = "paring";
            responseValueDefinition.ResponseElementName = "keha";
        }

        /// <summary>
        /// Allows each message protocol implementation to customize service description document
        /// before publishing.
        /// </summary>
        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENC, NamespaceConstants.SOAP_ENC);
        }
    }
}