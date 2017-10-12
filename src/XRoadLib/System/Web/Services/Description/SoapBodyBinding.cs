#if NETSTANDARD2_0

using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
{
    public class SoapBodyBinding : ServiceDescriptionFormatExtension
    {
        public string Encoding { get; set; }
        public string Namespace { get; set; }
        public SoapBindingUse Use { get; set; } = SoapBindingUse.Default;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP, "body", NamespaceConstants.SOAP);

            if (!string.IsNullOrEmpty(Encoding))
                writer.WriteAttributeString("encodingStyle", Encoding);

            if (!string.IsNullOrEmpty(Namespace))
                writer.WriteAttributeString("namespace", Namespace);

            if (Use != SoapBindingUse.Default)
                writer.WriteAttributeString("use", Use == SoapBindingUse.Encoded ? "encoded" : "literal");

            writer.WriteEndElement();
        }
    }
}

#endif
