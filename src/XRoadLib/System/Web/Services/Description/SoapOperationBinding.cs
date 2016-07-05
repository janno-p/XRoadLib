#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class SoapOperationBinding : ServiceDescriptionFormatExtension
    {
        public string SoapAction { get; set; }
        public SoapBindingStyle Style { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif
