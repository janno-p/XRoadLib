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

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(Prefix, "title", Namespace);

            if (!string.IsNullOrWhiteSpace(Language))
                writer.WriteAttributeString("lang", NamespaceConstants.Xml, Language);

            writer.WriteString(Text);

            writer.WriteEndElement();
        }
    }
}