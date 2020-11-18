using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class SoapAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap, "address", NamespaceConstants.Soap).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(null, "location", null, Location).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}