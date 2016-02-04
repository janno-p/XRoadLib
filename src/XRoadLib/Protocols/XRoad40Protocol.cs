using System.Web.Services.Description;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad40Protocol : XRoadProtocol<XRoadHeader40>
    {
        protected override string XRoadPrefix => PrefixConstants.XROAD;
        protected override string XRoadNamespace => NamespaceConstants.XROAD_V4;

        public override string Name => "4.0";

        public XRoad40Protocol(string producerName, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerName, style ?? new DocLiteralStyle(), schemaExporter)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddMandatoryHeaderElement(x => x.Client);
            AddMandatoryHeaderElement(x => x.Service);
            AddMandatoryHeaderElement(x => x.UserId);
            AddMandatoryHeaderElement(x => x.Id);
            AddMandatoryHeaderElement(x => x.Issue);
        }

        protected override void WriteSoapHeader(XmlWriter writer, XRoadHeader40 header)
        {
            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4);

            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4_ID) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.ID, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4_ID);

            if (MandatoryHeaders.Contains("client") || header.Client != null)
            {
                var element = new XElement(XName.Get("client", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), string.IsNullOrWhiteSpace(header.Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header.Client.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), header.Client.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), header.Client.MemberCode));
                if (!string.IsNullOrWhiteSpace(header.Client.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), header.Client.SubsystemCode));
                element.WriteTo(writer);
            }

            if (MandatoryHeaders.Contains("service") || header.Service != null)
            {
                var element = new XElement(XName.Get("service", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "SERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header.Service.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), header.Service.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), header.Service.MemberCode));
                if (!string.IsNullOrWhiteSpace(header.Service.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), header.Service.SubsystemCode));
                element.Add(new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), header.Service.ServiceCode));
                if (!string.IsNullOrWhiteSpace(header.Service.ServiceVersion))
                    element.Add(new XElement(XName.Get("serviceVersion", NamespaceConstants.XROAD_V4_ID), header.Service.ServiceVersion));
                element.WriteTo(writer);
            }

            if (MandatoryHeaders.Contains("centralService") || header.CentralService != null)
            {
                var element = new XElement(XName.Get("centralService", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "CENTRALSERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header.CentralService.XRoadInstance),
                    new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), header.CentralService.ServiceCode));
                element.WriteTo(writer);
            }

            WriteHeaderElement(writer, "id", header.Id);
            WriteHeaderElement(writer, "userId", header.UserId);
            WriteHeaderElement(writer, "issue", header.Issue);
            WriteHeaderElement(writer, "protocolVersion", header.ProtocolVersion);
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            var servicePort = serviceDescription.Services[0].Ports[0];

            var soapAddressBinding = (SoapAddressBinding)servicePort.Extensions[0];
            if (string.IsNullOrWhiteSpace(soapAddressBinding.Location))
                soapAddressBinding.Location = "http://INSERT_CORRECT_SERVICE_URL";
        }

        internal override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD_V4.Equals(ns) || NamespaceConstants.XROAD_V4_REPR.Equals(ns);
        }
    }
}