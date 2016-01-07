using System;
using System.Text;
using System.Text.RegularExpressions;

namespace XRoadLib
{
    public sealed class XRoadServiceName
    {
        public string Producer { get; }
        public string Method { get; }
        public uint? Version { get; }

        public XRoadServiceName(string fullServiceName)
        {
            var tuple = Parse(fullServiceName);
            if (tuple == null)
                return;

            Producer = tuple.Item1;
            Method = tuple.Item2;
            Version = tuple.Item3;
        }

        public XRoadServiceName(string producer, string method, uint? version = null)
        {
            Producer = producer;
            Method = method;
            Version = version;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Producer))
                sb.Append(Producer).Append(".");

            sb.Append(Method ?? string.Empty);

            if (Version.HasValue)
                sb.AppendFormat(".v{0}", Version.Value);

            return sb.ToString();
        }

        private static Tuple<string, string, uint?> Parse(string fullServiceName)
        {
            if (fullServiceName == null)
                return null;

            var matchMethodAndVersion = new Regex(@"^(?<method>[^\.]*)\.v(?<version>\d+)$").Match(fullServiceName);
            if (matchMethodAndVersion.Success)
                return Tuple.Create((string)null, matchMethodAndVersion.Groups["method"].Value, (uint?)Convert.ToUInt32(matchMethodAndVersion.Groups["version"].Value));

            var matchAll = new Regex(@"^((?<producer>[^\.]+)\.)?(?<method>[^\.]*)(\.v(?<version>\d+))?$").Match(fullServiceName);
            if (!matchAll.Success)
                return null;

            var method = matchAll.Groups["method"].Value;
            var producer = !string.IsNullOrWhiteSpace(matchAll.Groups["producer"].Value) ? matchAll.Groups["producer"].Value : null;

            uint versionOut;
            var version = uint.TryParse(matchAll.Groups["version"].Value, out versionOut) ? (uint?)versionOut : null;

            return Tuple.Create(producer, method, version);
        }
    }
}