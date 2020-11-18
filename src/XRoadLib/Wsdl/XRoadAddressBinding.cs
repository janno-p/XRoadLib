using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class XRoadAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Producer { get; set; } = string.Empty;

        public XRoadAddressBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(Prefix, "address", Namespace).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "producer", null, Producer).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}