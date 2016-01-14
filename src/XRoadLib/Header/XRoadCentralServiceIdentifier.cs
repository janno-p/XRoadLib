using System;
using System.Text;
using System.Xml.Serialization;

namespace XRoadLib.Header
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadCentralServiceIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadCentralServiceIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; internal set; }
        string IXRoadIdentifier.MemberClass { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.MemberCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.SubsystemCode { get { throw new NotImplementedException(); } }
        public string ServiceCode { get; internal set; }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }
        public XRoadObjectType ObjectType { get; internal set; }

        public override string ToString()
        {
            return new StringBuilder("CENTRALSERVICE:").Append(string.IsNullOrWhiteSpace(XRoadInstance) ? XRoadInstance : "XX")
                                                       .Append("/")
                                                       .Append(string.IsNullOrWhiteSpace(ServiceCode) ? ServiceCode : "_")
                                                       .ToString();
        }
    }
}