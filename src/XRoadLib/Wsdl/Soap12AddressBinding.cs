using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12AddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap12, "address", NamespaceConstants.Soap12).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "location", null, Location).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}