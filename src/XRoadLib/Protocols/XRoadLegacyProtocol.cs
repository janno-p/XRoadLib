using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public interface IXRoadLegacyProtocol
    {
        string XRoadNamespace { get; }
    }

    public abstract class XRoadLegacyProtocol : XRoadProtocol, IXRoadLegacyProtocol
    {
        public string ProducerName { get; }
        public IDictionary<string, string> Titles { get; } = new Dictionary<string, string>();

        public override bool NonTechnicalFaultInResponseElement => true;

        string IXRoadLegacyProtocol.XRoadNamespace => XRoadNamespace;

        protected XRoadLegacyProtocol(string producerName, string producerNamespace, Style style, ISchemaExporter schemaExporter)
            : base(producerNamespace, style, schemaExporter)
        {
            if (string.IsNullOrWhiteSpace(producerName))
                throw new ArgumentNullException(nameof(producerName));
            ProducerName = producerName;
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

#if NETSTANDARD1_5
            var address = new XRoadAddressBinding(XRoadPrefix, XRoadNamespace) { Producer = ProducerName };
#else
            var address = document.CreateElement(XRoadPrefix, "address", XRoadNamespace);
            address.SetAttribute("producer", ProducerName);
#endif

            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Extensions.Add(address);

            foreach (var title in Titles.OrderBy(t => t.Key.ToLower()))
            {
#if NETSTANDARD1_5
                var titleBinding = new XRoadTitleBinding(XRoadPrefix, XRoadNamespace)
                {
                    Language = title.Key,
                    Text = title.Value
                };
#else
                var titleBinding = document.CreateElement(XRoadPrefix, "title", XRoadNamespace);
                titleBinding.InnerText = title.Value;

                if (!string.IsNullOrWhiteSpace(title.Key))
                {
                    var attribute = document.CreateAttribute("xml", "lang", NamespaceConstants.XML);
                    attribute.Value = title.Key;
                    titleBinding.Attributes.Append(attribute);
                }
#endif

                servicePort.Extensions.Add(titleBinding);
            }

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            if (string.IsNullOrWhiteSpace(soapAddressBinding.Location))
                soapAddressBinding.Location = "http://TURVASERVER/cgi-bin/consumer_proxy";
        }
    }
}