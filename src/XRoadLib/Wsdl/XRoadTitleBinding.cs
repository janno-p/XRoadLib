using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class XRoadTitleBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Text { get; set; }
        public string Language { get; set; }

        public XRoadTitleBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }

        internal override async Task WriteAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(Prefix, "title", Namespace).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(Language))
                await writer.WriteAttributeStringAsync(null, "lang", NamespaceConstants.Xml, Language).ConfigureAwait(false);

            await writer.WriteStringAsync(Text).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }
    }
}