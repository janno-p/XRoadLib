using System;
using System.Xml.Serialization;

namespace XRoadLib.Protocol
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadCentralServiceIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadCentralServiceIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; }
        string IXRoadIdentifier.MemberClass { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.MemberCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.SubsystemCode { get { throw new NotImplementedException(); } }
        public string ServiceCode { get; }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }
        public XRoadObjectType ObjectType { get; }
    }
}