using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class XRoadAddressBinding : ServiceDescriptionFormatExtension
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string Prefix { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string Namespace { get; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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