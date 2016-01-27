using XRoadLib.Protocols.Headers;
using XRoadLib.Protocols.Styles;

namespace XRoadLib.Protocols
{
    public class XRoad31Protocol : LegacyProtocol<XRoadHeader31>
    {
        protected override string XRoadPrefix => PrefixConstants.XROAD;

        public override string XRoadNamespace => NamespaceConstants.XROAD;
        public override string Name => "3.1";

        public XRoad31Protocol(string producerName, string producerNamespace)
            : this(producerName, producerNamespace, new DocLiteralStyle())
        { }

        public XRoad31Protocol(string producerName, string producerNamespace, Style style)
            : base(producerName, producerNamespace, style)
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