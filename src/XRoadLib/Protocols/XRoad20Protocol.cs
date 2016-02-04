using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad20Protocol : XRoadLegacyProtocol<XRoadHeader20>
    {
        protected override string XRoadPrefix => PrefixConstants.XTEE;
        protected override string XRoadNamespace => NamespaceConstants.XTEE;

        public override string Name => "2.0";

        internal override string RequestPartNameInRequest => "keha";
        internal override string RequestPartNameInResponse => "paring";
        internal override string ResponsePartNameInResponse => "keha";

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

        protected override void WriteSoapHeader(XmlWriter writer, XRoadHeader20 header)
        {
            if (writer.LookupPrefix(NamespaceConstants.XTEE) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XTEE, NamespaceConstants.XMLNS, NamespaceConstants.XTEE);

            WriteHeaderElement(writer, "asutus", header.Asutus);
            WriteHeaderElement(writer, "andmekogu", header.Andmekogu);
            WriteHeaderElement(writer, "isikukood", header.Isikukood);
            WriteHeaderElement(writer, "toimik", header.Toimik);
            WriteHeaderElement(writer, "nimi", header.Nimi);
            WriteHeaderElement(writer, "ametnik", header.Ametnik);
            WriteHeaderElement(writer, "id", header.Id);
            WriteHeaderElement(writer, "allasutus", header.Allasutus);
            WriteHeaderElement(writer, "amet", header.Amet);
            WriteHeaderElement(writer, "ametniknimi", header.AmetnikNimi);
            WriteHeaderElement(writer, "asynkroonne", header.Asünkroonne);
            WriteHeaderElement(writer, "autentija", header.Autentija);
            WriteHeaderElement(writer, "makstud", header.Makstud);
            WriteHeaderElement(writer, "salastada", header.Salastada);
            WriteHeaderElement(writer, "salastada_sertifikaadiga", header.SalastadaSertifikaadiga);
            WriteHeaderElement(writer, "salastatud", header.Salastatud);
            WriteHeaderElement(writer, "salastatud_sertifikaadiga", header.SalastatudSertifikaadiga);
        }

        internal override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XTEE.Equals(ns);
        }

        internal override bool IsDefinedByEnvelope(XmlReader reader)
        {
            var attributeValue = reader.GetAttribute("encodingStyle", NamespaceConstants.SOAP_ENV);

            return NamespaceConstants.SOAP_ENC.Equals(attributeValue);
        }

        public override void ExportServiceDescription(ServiceDescription serviceDescription)
        {
            base.ExportServiceDescription(serviceDescription);

            serviceDescription.Namespaces.Add(PrefixConstants.SOAP_ENC, NamespaceConstants.SOAP_ENC);
        }

        internal override void WriteSoapEnvelope(XmlWriter writer)
        {
            base.WriteSoapEnvelope(writer);

            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC);
            writer.WriteAttributeString("encodingStyle", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC);
        }
    }
}