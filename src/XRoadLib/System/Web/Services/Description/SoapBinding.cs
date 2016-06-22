#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class SoapBinding : ServiceDescriptionFormatExtension
    {
        public SoapBindingStyle Style { get; set; }
        public string Transport { get; set; }
    }
}

#endif
