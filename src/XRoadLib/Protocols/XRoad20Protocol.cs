using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;

namespace XRoadLib.Protocols
{
    public class XRoad20Protocol : LegacyProtocol<XRoadHeader20>
    {
        protected override string XRoadPrefix => PrefixConstants.XTEE;
        protected override string RequestPartName => "keha";
        protected override string ResponsePartName => "keha";

        public override string XRoadNamespace => NamespaceConstants.XTEE;
        public override string Name => "2.0";

        public XRoad20Protocol(string producerName, string producerNamespace)
            : this(producerName, producerNamespace, new RpcEncodedStyle())
        { }

        public XRoad20Protocol(string producerName, string producerNamespace, Style style)
            : base(producerName, producerNamespace, style)
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
    }
}