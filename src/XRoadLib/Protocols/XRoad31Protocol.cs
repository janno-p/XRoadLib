using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad31Protocol : XRoadProtocol
    {
        public override string Name => "3.1";

        public XRoad31Protocol(string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }

        protected override void WriteXRoadHeader(XmlWriter writer, IXRoadHeader header)
        {
            var header31 = header as IXRoadHeader31;
            if (header31 == null)
                return;

            if (writer.LookupPrefix(NamespaceConstants.XROAD) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD);

            WriteHeaderElement(writer, "consumer", header31.Consumer, stringTypeName);
            WriteHeaderElement(writer, "producer", header31.Producer, stringTypeName);
            WriteHeaderElement(writer, "userId", header31.UserId, stringTypeName);
            WriteHeaderElement(writer, "issue", header31.Issue, stringTypeName);
            WriteHeaderElement(writer, "service", header31.Service, stringTypeName);
            WriteHeaderElement(writer, "id", header31.Id, stringTypeName);
            WriteHeaderElement(writer, "unit", header31.Unit, stringTypeName);
            WriteHeaderElement(writer, "position", header31.Position, stringTypeName);
            WriteHeaderElement(writer, "userName", header31.UserName, stringTypeName);
            WriteHeaderElement(writer, "async", header31.Async, booleanTypeName);
            WriteHeaderElement(writer, "authenticator", header31.Authenticator, stringTypeName);
            WriteHeaderElement(writer, "paid", header31.Paid, stringTypeName);
            WriteHeaderElement(writer, "encrypt", header31.Encrypt, stringTypeName);
            WriteHeaderElement(writer, "encryptCert", header31.EncryptCert, base64TypeName);
            WriteHeaderElement(writer, "encrypted", header31.Encrypted, stringTypeName);
            WriteHeaderElement(writer, "encryptedCert", header31.EncryptedCert, stringTypeName);
        }

        internal override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD.Equals(ns);
        }
    }
}