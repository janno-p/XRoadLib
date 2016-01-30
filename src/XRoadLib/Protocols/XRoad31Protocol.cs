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
            AddMandatoryHeaderElement(x => x.Service);
            AddMandatoryHeaderElement(x => x.UserId);
            AddMandatoryHeaderElement(x => x.Id);
            AddMandatoryHeaderElement(x => x.UserName);
        }

        public override bool IsHeaderNamespace(string ns)
        {
            return NamespaceConstants.XROAD.Equals(ns);
        }
    }
}