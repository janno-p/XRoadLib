#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class SoapBinding : ServiceDescriptionFormatExtension
    {
        public SoapBindingStyle Style { get; set; }
        public string Transport { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif
