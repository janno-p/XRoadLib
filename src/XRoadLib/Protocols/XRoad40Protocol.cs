using System;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

#if !NETSTANDARD1_5
using System.Web.Services.Description;
#endif

namespace XRoadLib.Protocols
{
    public class XRoad40Protocol : XRoadProtocol
    {
        protected override string XRoadPrefix => PrefixConstants.XROAD;
        protected override string XRoadNamespace => NamespaceConstants.XROAD_V4;

        public override string Name => "4.0";

        public XRoad40Protocol(string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddHeaderElement(x => x.Client);
            AddHeaderElement(x => x.Service);
            AddHeaderElement(x => x.UserId);
            AddHeaderElement(x => x.Id);
            AddHeaderElement(x => x.Issue);
        }

        protected void AddHeaderElement<T>(Expression<Func<XRoadHeader40, T>> headerFunc)
        {
            AddMandatoryHeaderElement(headerFunc);
        }

        protected override void WriteXRoadHeader(XmlWriter writer, IXRoadHeader header)
        {
            var header40 = header as IXRoadHeader40;
            if (header40 == null)
                return;

            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4);

            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4_ID) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.ID, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4_ID);

            if (MandatoryHeaders.Contains("client") || header40.Client != null)
            {
                var element = new XElement(XName.Get("client", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), string.IsNullOrWhiteSpace(header40.Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header40.Client.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), header40.Client.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), header40.Client.MemberCode));
                if (!string.IsNullOrWhiteSpace(header40.Client.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), header40.Client.SubsystemCode));
                element.WriteTo(writer);
            }

            if (MandatoryHeaders.Contains("service") || header40.Service != null)
            {
                var element = new XElement(XName.Get("service", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "SERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header40.Service.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), header40.Service.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), header40.Service.MemberCode));
                if (!string.IsNullOrWhiteSpace(header40.Service.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), header40.Service.SubsystemCode));
                element.Add(new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), header40.Service.ServiceCode));
                if (!string.IsNullOrWhiteSpace(header40.Service.ServiceVersion))
                    element.Add(new XElement(XName.Get("serviceVersion", NamespaceConstants.XROAD_V4_ID), header40.Service.ServiceVersion));
                element.WriteTo(writer);
            }

            if (MandatoryHeaders.Contains("centralService") || header40.CentralService != null)
            {
                var element = new XElement(XName.Get("centralService", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "CENTRALSERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), header40.CentralService.XRoadInstance),
                    new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), header40.CentralService.ServiceCode));
                element.WriteTo(writer);
            }

            WriteHeaderElement(writer, "id", header40.Id, stringTypeName);
            WriteHeaderElement(writer, "userId", header40.UserId, stringTypeName);
            WriteHeaderElement(writer, "issue", header40.Issue, stringTypeName);
            WriteHeaderElement(writer, "protocolVersion", header40.ProtocolVersion, stringTypeName);
        }

        internal override IXRoadHeader CreateHeader()
        {
            return new XRoadHeader40();
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