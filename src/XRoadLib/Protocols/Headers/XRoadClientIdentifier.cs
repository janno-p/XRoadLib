using System;
using System.Text;
using System.Xml.Serialization;

namespace XRoadLib.Protocols.Headers
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadClientIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadClientIdentifier : IXRoadIdentifier
    {
        public string XRoadInstance { get; set; }
        public string MemberClass { get; set; }
        public string MemberCode { get; set; }
        public string SubsystemCode { get; set; } // Optional
        string IXRoadIdentifier.ServiceCode { get { throw new NotImplementedException(); } }
        string IXRoadIdentifier.ServiceVersion { get { throw new NotImplementedException(); } }

        public XRoadObjectType ObjectType { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(XRoadInstance) && string.IsNullOrEmpty(MemberClass) && string.IsNullOrEmpty(SubsystemCode))
                return MemberCode ?? "";

            var sb = new StringBuilder($"{XRoadInstance}/{MemberClass}/{MemberCode}");
            if (!string.IsNullOrEmpty(SubsystemCode))
                sb.Append($"/{SubsystemCode}");

            return sb.ToString();
        }
    }
}