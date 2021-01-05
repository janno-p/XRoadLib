using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class XRoadTitleBinding : ServiceDescriptionFormatExtension
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string Prefix { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public string Namespace { get; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Text { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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