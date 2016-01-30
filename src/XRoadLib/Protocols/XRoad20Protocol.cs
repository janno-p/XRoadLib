using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad20Protocol : LegacyProtocol<XRoadHeader20>
    {
        protected override string XRoadPrefix => PrefixConstants.XTEE;
        protected override string XRoadNamespace => NamespaceConstants.XTEE;

        public override string Name => "2.0";
        public override string RequestPartNameInRequest => "keha";
        public override string RequestPartNameInResponse => "paring";
        public override string ResponsePartNameInResponse => "keha";

        public XRoad20Protocol(string producerName, string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerName, producerNamespace, style ?? new RpcEncodedStyle(), schemaExporter)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddMandatoryHeaderElement(x => x.Asutus);
            AddMandatoryHeaderElement(x => x.Andmekogu);
            AddMandatoryHeaderElement(x => x.Nimi);
            AddMandatoryHeaderElement(x => x.Isikukood);
            AddMandatoryHeaderElement(x => x.Id);
            AddMandatoryHeaderElement(x => x.AmetnikNimi);
        }

        public override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XTEE.Equals(ns);
        }

        public override bool IsDefinedByEnvelope(XmlReader reader)
        {
            var attributeValue = reader.GetAttribute("encodingStyle", NamespaceConstants.SOAP_ENV);

            return NamespaceConstants.SOAP_ENC.Equals(attributeValue);
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENC, NamespaceConstants.SOAP_ENC);
        }
    }
}