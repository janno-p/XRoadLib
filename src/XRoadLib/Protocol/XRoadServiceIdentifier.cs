using System.Xml.Serialization;

namespace XRoadLib.Protocol
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadServiceIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadServiceIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; }
        public string MemberClass { get; }
        public string MemberCode { get; }
        public string SubsystemCode { get; } // Optional
        public string ServiceCode { get; }
        public string ServiceVersion { get; } // Optional
        public XRoadObjectType ObjectType { get; }
    }
}