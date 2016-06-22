#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class SoapHeaderBinding : ServiceDescriptionFormatExtension
    {
        public string Encoding { get; set; }
        public XmlQualifiedName Message { get; set; }
        public string Namespace { get; set; }
        public string Part { get; set; }
        public SoapBindingUse Use { get; set; }
    }
}

#endif
