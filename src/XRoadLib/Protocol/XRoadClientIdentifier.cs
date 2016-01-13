using System;
using System.Xml.Serialization;

namespace XRoadLib.Protocol
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadClientIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadClientIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; }
        public string MemberClass { get; }
        public string MemberCode { get; }
        public string SubsystemCode { get; } // Optional
        string IXRoadIdentifier.ServiceCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }
        public XRoadObjectType ObjectType { get; }
    }
}