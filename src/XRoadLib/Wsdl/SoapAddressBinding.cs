using System.Xml;

namespace XRoadLib.Wsdl
{
    public class SoapAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Location { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP, "address", NamespaceConstants.SOAP);
            writer.WriteAttributeString("location", Location);
            writer.WriteEndElement();
        }
    }
}