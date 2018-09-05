using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12Binding : ServiceDescriptionFormatExtension
    {
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;
        public string Transport { get; set; } = string.Empty;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP12, "binding", NamespaceConstants.SOAP12);

            if (Style != SoapBindingStyle.Default)
                writer.WriteAttributeString("style", Style == SoapBindingStyle.Rpc ? "rpc" : "document");

            writer.WriteAttributeString("transport", Transport);

            writer.WriteEndElement();
        }
    }
}