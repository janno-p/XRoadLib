using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class SoapOperationBinding : ServiceDescriptionFormatExtension
    {
        public string SoapAction { get; set; } = string.Empty;
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap, "operation", NamespaceConstants.Soap).ConfigureAwait(false);

            await writer.WriteAttributeStringAsync(null, "soapAction", null, SoapAction).ConfigureAwait(false);

            if (Style != SoapBindingStyle.Default)
                await writer.WriteAttributeStringAsync(null, "style", null, Style == SoapBindingStyle.Rpc ? "rpc" : "document").ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}