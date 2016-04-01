using System;
using System.Xml.Serialization;

namespace XRoadLib.Protocols.Headers
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadCentralServiceIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadCentralServiceIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; set; }
        string IXRoadIdentifier.MemberClass { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.MemberCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.SubsystemCode { get { throw new NotImplementedException(); } }
        public string ServiceCode { get; set; }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }
        public XRoadObjectType ObjectType { get; set; }

        public override string ToString()
        {
            return $"{XRoadInstance}/{ServiceCode}";
        }
    }
}