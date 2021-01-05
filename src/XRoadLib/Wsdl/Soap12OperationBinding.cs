using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Soap12OperationBinding : ServiceDescriptionFormatExtension
    {
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string SoapAction { get; set; } = string.Empty;

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap12, "operation", NamespaceConstants.Soap12).ConfigureAwait(false);

            await writer.WriteAttributeStringAsync(null, "soapAction", null, SoapAction).ConfigureAwait(false);

            if (Style != SoapBindingStyle.Default)
                await writer.WriteAttributeStringAsync(null, "style", null, Style == SoapBindingStyle.Rpc ? "rpc" : "document").ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}