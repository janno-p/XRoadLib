using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Soap12OperationBinding : ServiceDescriptionFormatExtension
    {
        public string SoapAction { get; set; } = string.Empty;
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.Soap12, "operation", NamespaceConstants.Soap12);

            writer.WriteAttributeString("soapAction", SoapAction);

            if (Style != SoapBindingStyle.Default)
                writer.WriteAttributeString("style", Style == SoapBindingStyle.Rpc ? "rpc" : "document");

            writer.WriteEndElement();
        }
    }
}