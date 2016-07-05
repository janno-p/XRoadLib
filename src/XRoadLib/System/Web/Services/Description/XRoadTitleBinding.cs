#if NETSTANDARD1_5

using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
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

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(Prefix, "title", Namespace);

            if (!string.IsNullOrWhiteSpace(Language))
                writer.WriteAttributeString("lang", NamespaceConstants.XML, Language);

            writer.WriteString(Text);

            writer.WriteEndElement();
        }
    }
}

#endif