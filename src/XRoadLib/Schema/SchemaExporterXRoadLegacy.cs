using System.Reflection;
using XRoadLib.Wsdl;

namespace XRoadLib.Schema
{
    /// <inheritdoc />
    public abstract class SchemaExporterXRoadLegacy : AbstractSchemaExporter
    {
        private readonly Assembly _contractAssembly;

        /// <summary>
        /// X-Road producer name for legacy protocols.
        /// </summary>
        protected string ProducerName { get; }

        /// <inheritdoc />
        protected SchemaExporterXRoadLegacy(string producerName, Assembly contractAssembly, string producerNamespace)
            : base(producerNamespace)
        {
            _contractAssembly = contractAssembly;
            ProducerName = producerName;
        }

        /// <inheritdoc />
        public override void ExportResponseDefinition(ResponseDefinition responseDefinition)
        {
            base.ExportResponseDefinition(responseDefinition);

            responseDefinition.ContainsNonTechnicalFault = true;
        }

        /// <inheritdoc />
        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            if (!serviceDescription.Namespaces.ContainsKey(XRoadPrefix))
                serviceDescription.Namespaces.Add(XRoadPrefix, XRoadNamespace);

            var address = new XRoadAddressBinding(XRoadPrefix, XRoadNamespace) { Producer = ProducerName };

            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Extensions.Add(address);

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            soapAddressBinding.Location = "http://TURVASERVER/cgi-bin/consumer_proxy";

            AddXRoadTitles(servicePort);
        }

        /// <summary>
        /// Define X-Road titles of the service port.
        /// </summary>
        protected virtual void AddXRoadTitles(Port servicePort)
        { }

        /// <summary>
        /// Adds X-Road title element to service port.
        /// </summary>
        protected void AddXRoadTitle(Port servicePort, string language, string title)
        {
            servicePort.Extensions.Add(new XRoadTitleBinding(XRoadPrefix, XRoadNamespace)
            {
                Language = language,
                Text = title
            });
        }

        /// <summary>
        /// Configure protocol global settings.
        /// </summary>
        public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
        {
            base.ExportProtocolDefinition(protocolDefinition);

            protocolDefinition.ContractAssembly = _contractAssembly;
            protocolDefinition.TechNotesElementName = "technotes";
        }
    }
}