using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12AddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP12, "address", NamespaceConstants.SOAP12);
            writer.WriteAttributeString("location", Location);
            writer.WriteEndElement();
        }
    }
}