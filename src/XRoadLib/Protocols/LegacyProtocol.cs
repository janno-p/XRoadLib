using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;

namespace XRoadLib.Protocols
{
    public interface ILegacyProtocol
    {
        string XRoadNamespace { get; }
    }

    public abstract class LegacyProtocol<THeader> : Protocol<THeader>, ILegacyProtocol where THeader : IXRoadHeader, new()
    {
        public string ProducerName { get; }
        public IDictionary<string, string> Titles { get; } = new Dictionary<string, string>();

        string ILegacyProtocol.XRoadNamespace => XRoadNamespace;

        protected LegacyProtocol(string producerName, string producerNamespace, Style style)
            : base(producerNamespace, style)
        {
            if (string.IsNullOrWhiteSpace(producerName))
                throw new ArgumentNullException(nameof(producerName));
            ProducerName = producerName;
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            var address = document.CreateElement(XRoadPrefix, "address", XRoadNamespace);
            address.SetAttribute("producer", ProducerName);

            var servicePort = serviceDescription.Services[0].Ports[0];
            servicePort.Extensions.Add(address);

            foreach (var title in Titles.OrderBy(t => t.Key.ToLower()))
            {
                var titleElement = document.CreateElement(XRoadPrefix, "title", XRoadNamespace);
                titleElement.InnerText = title.Value;
                servicePort.Extensions.Add(titleElement);

                if (string.IsNullOrWhiteSpace(title.Key))
                    continue;

                var attribute = document.CreateAttribute("xml", "lang", NamespaceConstants.XML);
                attribute.Value = title.Key;
                titleElement.Attributes.Append(attribute);
            }

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            if (string.IsNullOrWhiteSpace(soapAddressBinding.Location))
                soapAddressBinding.Location = "http://TURVASERVER/cgi-bin/consumer_proxy";
        }
    }
}