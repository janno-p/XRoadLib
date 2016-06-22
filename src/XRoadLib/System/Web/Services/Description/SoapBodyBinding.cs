#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class SoapBodyBinding : ServiceDescriptionFormatExtension
    {
        public string Encoding { get; set; }
        public string Namespace { get; set; }
        public SoapBindingUse Use { get; set; }
    }
}

#endif
