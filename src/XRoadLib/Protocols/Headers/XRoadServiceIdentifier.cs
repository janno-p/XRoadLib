using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace XRoadLib.Protocols.Headers
{
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadServiceIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public class XRoadServiceIdentifier : IXRoadIdentifier
    {
        private string serviceVersion;

        public string XRoadInstance { get; set; }
        public string MemberClass { get; set; }
        public string MemberCode { get; set; }
        public string SubsystemCode { get; set; } // Optional
        public string ServiceCode { get; set; }

        // Optional
        public string ServiceVersion
        {
            get { return serviceVersion; }
            set
            {
                serviceVersion = value;
                Version = string.IsNullOrWhiteSpace(value) ? (uint?)null : Convert.ToUInt32(value.Substring(1));
            }
        }

        public XRoadObjectType ObjectType { get; set; }

        public uint? Version { get; private set; }

        public static XRoadServiceIdentifier FromString(string serviceName)
        {
            if (serviceName == null)
                return new XRoadServiceIdentifier();

            var matchMethodAndVersion = new Regex(@"^(?<method>[^\.]*)\.(?<version>v\d+)$").Match(serviceName);
            if (matchMethodAndVersion.Success)
                return new XRoadServiceIdentifier
                {
                    ServiceCode = matchMethodAndVersion.Groups["method"].Value,
                    ServiceVersion = matchMethodAndVersion.Groups["version"].Success ? matchMethodAndVersion.Groups["version"].Value : null
                };

            var matchAll = new Regex(@"^((?<producer>[^\.]+)\.)?(?<method>[^\.]*)(\.(?<version>v\d+))?$").Match(serviceName);
            if (!matchAll.Success)
                return new XRoadServiceIdentifier();

            return new XRoadServiceIdentifier
            {
                ServiceCode = matchAll.Groups["method"].Value,
                ServiceVersion = matchAll.Groups["version"].Success ? matchAll.Groups["version"].Value : null
            };
        }

        public string ToFullName()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(SubsystemCode))
                sb.Append($"{SubsystemCode}.");

            sb.Append(ServiceCode ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(ServiceVersion))
                sb.Append($".{ServiceVersion}");

            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(XRoadInstance) || !string.IsNullOrEmpty(MemberClass) || !string.IsNullOrEmpty(MemberCode))
                sb.Append($"{XRoadInstance}/{MemberClass}/{MemberCode}/");

            sb.Append($"{SubsystemCode}/{ServiceCode}");
            if (!string.IsNullOrEmpty(ServiceVersion))
                sb.Append($"/{ServiceVersion}");

            return sb.ToString();
        }
    }
}