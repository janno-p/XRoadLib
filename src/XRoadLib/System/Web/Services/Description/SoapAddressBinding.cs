#if NETSTANDARD2_0

using System.Xml;
using XRoadLib;

namespace System.Web.Services.Description
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

#endif
