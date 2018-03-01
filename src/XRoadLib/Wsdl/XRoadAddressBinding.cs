using System.Xml;

namespace XRoadLib.Wsdl
{
    public class XRoadAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Producer { get; set; } = string.Empty;

        public XRoadAddressBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(Prefix, "address", Namespace);
            writer.WriteAttributeString("producer", Producer);
            writer.WriteEndElement();
        }
    }
}