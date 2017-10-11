#if NETSTANDARD1_6

using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
{
    public class SoapOperationBinding : ServiceDescriptionFormatExtension
    {
        public string SoapAction { get; set; } = string.Empty;
        public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;

        internal override void Write(XmlWriter writer)
        {
            writer.WriteStartElement(PrefixConstants.SOAP, "operation", NamespaceConstants.SOAP);

            writer.WriteAttributeString("soapAction", SoapAction);

            if (Style != SoapBindingStyle.Default)
                writer.WriteAttributeString("style", Style == SoapBindingStyle.Rpc ? "rpc" : "document");

            writer.WriteEndElement();
        }
    }
}

#endif
