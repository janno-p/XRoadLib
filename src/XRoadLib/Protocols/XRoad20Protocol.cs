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

            WriteHeaderElement(writer, "asutus", header20.Asutus, stringTypeName);
            WriteHeaderElement(writer, "andmekogu", header20.Andmekogu, stringTypeName);
            WriteHeaderElement(writer, "isikukood", header20.Isikukood, stringTypeName);
            WriteHeaderElement(writer, "toimik", header20.Toimik, stringTypeName);
            WriteHeaderElement(writer, "nimi", header20.Nimi, stringTypeName);
            WriteHeaderElement(writer, "ametnik", header20.Ametnik, stringTypeName);
            WriteHeaderElement(writer, "id", header20.Id, stringTypeName);
            WriteHeaderElement(writer, "allasutus", header20.Allasutus, stringTypeName);
            WriteHeaderElement(writer, "amet", header20.Amet, stringTypeName);
            WriteHeaderElement(writer, "ametniknimi", header20.AmetnikNimi, stringTypeName);
            WriteHeaderElement(writer, "asynkroonne", header20.Asünkroonne, booleanTypeName);
            WriteHeaderElement(writer, "autentija", header20.Autentija, stringTypeName);
            WriteHeaderElement(writer, "makstud", header20.Makstud, stringTypeName);
            WriteHeaderElement(writer, "salastada", header20.Salastada, stringTypeName);
            WriteHeaderElement(writer, "salastada_sertifikaadiga", header20.SalastadaSertifikaadiga, base64TypeName);
            WriteHeaderElement(writer, "salastatud", header20.Salastatud, stringTypeName);
            WriteHeaderElement(writer, "salastatud_sertifikaadiga", header20.SalastatudSertifikaadiga, stringTypeName);
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

        internal override void WriteSoapEnvelope(XmlWriter writer)
        {
            base.WriteSoapEnvelope(writer);

            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC);
            writer.WriteAttributeString("encodingStyle", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC);
        }
    }
}