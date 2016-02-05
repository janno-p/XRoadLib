using System;
using System.Linq.Expressions;
using System.Web.Services.Description;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad20Protocol : XRoadLegacyProtocol
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
            AddHeaderElement(x => x.Asutus);
            AddHeaderElement(x => x.Andmekogu);
            AddHeaderElement(x => x.Nimi);
            AddHeaderElement(x => x.Isikukood);
            AddHeaderElement(x => x.Id);
            AddHeaderElement(x => x.AmetnikNimi);
        }

        protected void AddHeaderElement<T>(Expression<Func<XRoadHeader20, T>> headerFunc)
        {
            AddMandatoryHeaderElement(headerFunc);
        }

        protected override void WriteXRoadHeader(XmlWriter writer, IXRoadHeader header)
        {
            var header20 = header as IXRoadHeader20;
            if (header20 == null)
                return;

            if (writer.LookupPrefix(NamespaceConstants.XTEE) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XTEE, NamespaceConstants.XMLNS, NamespaceConstants.XTEE);

            WriteHeaderElement(writer, "asutus", header20.Asutus);
            WriteHeaderElement(writer, "andmekogu", header20.Andmekogu);
            WriteHeaderElement(writer, "isikukood", header20.Isikukood);
            WriteHeaderElement(writer, "toimik", header20.Toimik);
            WriteHeaderElement(writer, "nimi", header20.Nimi);
            WriteHeaderElement(writer, "ametnik", header20.Ametnik);
            WriteHeaderElement(writer, "id", header20.Id);
            WriteHeaderElement(writer, "allasutus", header20.Allasutus);
            WriteHeaderElement(writer, "amet", header20.Amet);
            WriteHeaderElement(writer, "ametniknimi", header20.AmetnikNimi);
            WriteHeaderElement(writer, "asynkroonne", header20.Asünkroonne);
            WriteHeaderElement(writer, "autentija", header20.Autentija);
            WriteHeaderElement(writer, "makstud", header20.Makstud);
            WriteHeaderElement(writer, "salastada", header20.Salastada);
            WriteHeaderElement(writer, "salastada_sertifikaadiga", header20.SalastadaSertifikaadiga);
            WriteHeaderElement(writer, "salastatud", header20.Salastatud);
            WriteHeaderElement(writer, "salastatud_sertifikaadiga", header20.SalastatudSertifikaadiga);
        }

        internal override IXRoadHeader CreateHeader()
        {
            return new XRoadHeader20();
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