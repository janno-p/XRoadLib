using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad31Protocol : LegacyProtocol<XRoadHeader31>
    {
        protected override string XRoadPrefix => PrefixConstants.XROAD;
        protected override string XRoadNamespace => NamespaceConstants.XROAD;

        public override string Name => "3.1";

        public XRoad31Protocol(string producerName, string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerName, producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddMandatoryHeaderElement(x => x.Consumer);
            AddMandatoryHeaderElement(x => x.Producer);
            AddMandatoryHeaderElement(x => ((IXRoadHeader31)x).Service);
            AddMandatoryHeaderElement(x => x.UserId);
            AddMandatoryHeaderElement(x => x.Id);
            AddMandatoryHeaderElement(x => x.UserName);
        }

        protected override void WriteSoapHeader(XmlWriter writer, XRoadHeader31 header)
        {
            if (writer.LookupPrefix(NamespaceConstants.XROAD) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD);

            WriteHeaderElement(writer, "consumer", header.Consumer);
            WriteHeaderElement(writer, "producer", header.Producer);
            WriteHeaderElement(writer, "userId", header.UserId);
            WriteHeaderElement(writer, "issue", header.Issue);
            WriteHeaderElement(writer, "service", ((IXRoadHeader31)header).Service);
            WriteHeaderElement(writer, "id", header.Id);
            WriteHeaderElement(writer, "unit", header.Unit);
            WriteHeaderElement(writer, "position", header.Position);
            WriteHeaderElement(writer, "userName", header.UserName);
            WriteHeaderElement(writer, "async", header.Async);
            WriteHeaderElement(writer, "authenticator", header.Authenticator);
            WriteHeaderElement(writer, "paid", header.Paid);
            WriteHeaderElement(writer, "encrypt", header.Encrypt);
            WriteHeaderElement(writer, "encryptCert", header.EncryptCert);
            WriteHeaderElement(writer, "encrypted", header.Encrypted);
            WriteHeaderElement(writer, "encryptedCert", header.EncryptedCert);
        }

        public override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD.Equals(ns);
        }
    }
}