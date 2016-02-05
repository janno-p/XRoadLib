using System;
using System.Linq.Expressions;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad31Protocol : XRoadLegacyProtocol
    {
        protected override string XRoadPrefix => PrefixConstants.XROAD;
        protected override string XRoadNamespace => NamespaceConstants.XROAD;

        public override string Name => "3.1";

        public XRoad31Protocol(string producerName, string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerName, producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }

        protected override void DefineMandatoryHeaderElements()
        {
            AddHeaderElement(x => x.Consumer);
            AddHeaderElement(x => x.Producer);
            AddHeaderElement(x => ((IXRoadHeader31)x).Service);
            AddHeaderElement(x => x.UserId);
            AddHeaderElement(x => x.Id);
            AddHeaderElement(x => x.UserName);
        }

        protected void AddHeaderElement<T>(Expression<Func<XRoadHeader31, T>> headerFunc)
        {
            AddMandatoryHeaderElement(headerFunc);
        }

        protected override void WriteXRoadHeader(XmlWriter writer, IXRoadHeader header)
        {
            var header31 = header as IXRoadHeader31;
            if (header31 == null)
                return;

            if (writer.LookupPrefix(NamespaceConstants.XROAD) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD);

            WriteHeaderElement(writer, "consumer", header31.Consumer);
            WriteHeaderElement(writer, "producer", header31.Producer);
            WriteHeaderElement(writer, "userId", header31.UserId);
            WriteHeaderElement(writer, "issue", header31.Issue);
            WriteHeaderElement(writer, "service", header31.Service);
            WriteHeaderElement(writer, "id", header31.Id);
            WriteHeaderElement(writer, "unit", header31.Unit);
            WriteHeaderElement(writer, "position", header31.Position);
            WriteHeaderElement(writer, "userName", header31.UserName);
            WriteHeaderElement(writer, "async", header31.Async);
            WriteHeaderElement(writer, "authenticator", header31.Authenticator);
            WriteHeaderElement(writer, "paid", header31.Paid);
            WriteHeaderElement(writer, "encrypt", header31.Encrypt);
            WriteHeaderElement(writer, "encryptCert", header31.EncryptCert);
            WriteHeaderElement(writer, "encrypted", header31.Encrypted);
            WriteHeaderElement(writer, "encryptedCert", header31.EncryptedCert);
        }

        internal override IXRoadHeader CreateHeader()
        {
            return new XRoadHeader31();
        }

        internal override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD.Equals(ns);
        }
    }
}