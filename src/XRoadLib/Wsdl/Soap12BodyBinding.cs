using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class Soap12BodyBinding : ServiceDescriptionFormatExtension
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Encoding { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Namespace { get; set; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public SoapBindingUse Use { get; set; } = SoapBindingUse.Default;

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(PrefixConstants.Soap12, "body", NamespaceConstants.Soap12).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(Encoding))
                await writer.WriteAttributeStringAsync(null, "encodingStyle", null, Encoding).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(Namespace))
                await writer.WriteAttributeStringAsync(null, "namespace", null, Namespace).ConfigureAwait(false);

            if (Use != SoapBindingUse.Default)
                await writer.WriteAttributeStringAsync( null, "use", null, Use == SoapBindingUse.Encoded ? "encoded" : "literal").ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}