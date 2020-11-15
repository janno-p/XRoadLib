using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12AddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.Soap12, "address", NamespaceConstants.Soap12);
            writer.WriteAttributeString("location", Location);
            writer.WriteEndElement();
        }
    }
}