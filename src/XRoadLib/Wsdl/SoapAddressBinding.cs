using System.Xml;

namespace XRoadLib.Wsdl
{
    public class SoapAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.Soap, "address", NamespaceConstants.Soap);
            writer.WriteAttributeString("location", Location);
            writer.WriteEndElement();
        }
    }
}