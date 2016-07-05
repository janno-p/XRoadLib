#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class SoapBodyBinding : ServiceDescriptionFormatExtension
    {
        public string Encoding { get; set; }
        public string Namespace { get; set; }
        public SoapBindingUse Use { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif
