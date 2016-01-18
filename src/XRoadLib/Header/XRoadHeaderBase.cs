using System.Xml;

namespace XRoadLib.Header
{
    internal abstract class XRoadHeaderBase : IXRoadHeader
    {
        public XRoadClientIdentifier Client { get; internal set; }
        public XRoadServiceIdentifier Service { get; internal set; }

        public string UserId { get; set; }
        public string Id { get; set; }
        public string Issue { get; set; }
        public string ProtocolVersion { get; set; }

        public abstract XRoadProtocol Protocol { get; }

        public abstract void SetHeaderValue(XmlReader reader);

        public virtual void Validate()
        { }

        public static XRoadHeaderBase FromNamespace(string ns)
        {
            switch (ns)
            {
                case NamespaceConstants.XTEE:
                    return new XRoadHeader20();
                case NamespaceConstants.XROAD:
                    return new XRoadHeader31();
                case NamespaceConstants.XROAD_V4:
                case NamespaceConstants.XROAD_V4_REPR:
                    return new XRoadHeader40();
                default:
                    return null;
            }
        }
    }
}