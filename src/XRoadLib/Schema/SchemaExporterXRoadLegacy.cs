using System.Web.Services.Description;

#if !NETSTANDARD1_5
using System.Xml;
#endif

namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol legacy version.
    /// </summary>
    public abstract class SchemaExporterXRoadLegacy : SchemaExporterBase
    {
#if !NETSTANDARD1_5
        private readonly XmlDocument document = new XmlDocument();
#endif

        /// <summary>
        /// X-Road producer name for legacy protocols.
        /// </summary>
        protected readonly string producerName;

        /// <summary>
        /// X-Road namespace preferred prefix.
        /// </summary>
        protected readonly string xRoadPrefix;

        /// <summary>
        /// X-Road namespace which defines X-Road protocol.
        /// </summary>
        protected readonly string xRoadNamespace;

        /// <summary>
        /// X-Road standard compliant producer namespace.
        /// </summary>
        public abstract string ProducerNamespace { get; }

        /// <summary>
        /// Initialize new legacy schema exporter.
        /// </summary>
        protected SchemaExporterXRoadLegacy(string producerName, string xRoadPrefix, string xRoadNamespace)
        {
            this.producerName = producerName;
            this.xRoadPrefix = xRoadPrefix;
            this.xRoadNamespace = xRoadNamespace;
        }

        /// <summary>
        /// Configure response elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition)
        {
            base.ExportResponseValueDefinition(responseValueDefinition);

            responseValueDefinition.ContainsNonTechnicalFault = true;
        }

        /// <summary>
        /// Allows each message protocol implementation to customize service description document
        /// before publishing.
        /// </summary>
        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            serviceDescription.Namespaces.Add(xRoadPrefix, xRoadNamespace);

#if NETSTANDARD1_5
            var address = new XRoadAddressBinding(xRoadPrefix, xRoadNamespace) { Producer = producerName };
#else
            var address = document.CreateElement(xRoadPrefix, "address", xRoadNamespace);
            address.SetAttribute("producer", producerName);
#endif

            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Extensions.Add(address);

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            soapAddressBinding.Location = "http://TURVASERVER/cgi-bin/consumer_proxy";
        }

        /// <summary>
        /// Adds X-Road title element to service port.
        /// </summary>
        protected void AddXRoadTitle(Port servicePort, string language, string title)
        {
#if NETSTANDARD1_5
            var titleBinding = new XRoadTitleBinding(xRoadPrefix, xRoadNamespace)
            {
                Language = language,
                Text = title
            };
#else
            var titleBinding = document.CreateElement(xRoadPrefix, "title", xRoadNamespace);
            titleBinding.InnerText = title;

            if (!string.IsNullOrWhiteSpace(language))
            {
                var attribute = document.CreateAttribute("xml", "lang", NamespaceConstants.XML);
                attribute.Value = language;
                titleBinding.Attributes.Append(attribute);
            }
#endif
            servicePort.Extensions.Add(titleBinding);
        }
    }
}