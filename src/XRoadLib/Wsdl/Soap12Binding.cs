using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Soap12Binding : ServiceDescriptionFormatExtension
    {
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;
        
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string Transport { get; set; } = string.Empty;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap12, "binding", NamespaceConstants.Soap12).ConfigureAwait(false);

            if (Style != SoapBindingStyle.Default)
                await writer.WriteAttributeStringAsync(null, "style", null, Style == SoapBindingStyle.Rpc ? "rpc" : "document").ConfigureAwait(false);

            await writer.WriteAttributeStringAsync(null, "transport", null, Transport).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}