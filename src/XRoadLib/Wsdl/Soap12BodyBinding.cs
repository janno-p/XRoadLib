using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12BodyBinding : ServiceDescriptionFormatExtension
    {
        public string Encoding { get; set; }
        public string Namespace { get; set; }
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