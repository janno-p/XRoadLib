using System.Xml;

namespace XRoadLib.Wsdl
{
    public class SoapBinding : ServiceDescriptionFormatExtension
    {
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;
        public string Transport { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP, "binding", NamespaceConstants.SOAP);

            if (Style != SoapBindingStyle.Default)
                writer.WriteAttributeString("style", Style == SoapBindingStyle.Rpc ? "rpc" : "document");

            writer.WriteAttributeString("transport", Transport);

            writer.WriteEndElement();
        }
    }
}