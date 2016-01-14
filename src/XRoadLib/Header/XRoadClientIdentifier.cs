using System;
using System.Text;
using System.Xml.Serialization;

namespace XRoadLib.Header
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadClientIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadClientIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; internal set; }
        public string MemberClass { get; internal set; }
        public string MemberCode { get; internal set; }
        public string SubsystemCode { get; internal set; } // Optional
        string IXRoadIdentifier.ServiceCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }

        public XRoadObjectType ObjectType { get; internal set; }

        public override string ToString()
        {
            var prefix = ObjectType == XRoadObjectType.Member ? "MEMBER:" : "SUBSYSTEM:";

            var sb = new StringBuilder(prefix).Append(string.IsNullOrWhiteSpace(XRoadInstance) ? XRoadInstance : "XX")
                                              .Append("/")
                                              .Append(string.IsNullOrWhiteSpace(MemberClass) ? MemberClass : "_")
                                              .Append("/")
                                              .Append(string.IsNullOrWhiteSpace(MemberCode) ? MemberCode : "_");

            if (!string.IsNullOrWhiteSpace(SubsystemCode))
                sb.Append($"/{SubsystemCode}");

            return sb.ToString();
        }
    }
}