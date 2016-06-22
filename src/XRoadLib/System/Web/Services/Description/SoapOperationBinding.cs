#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class SoapOperationBinding : ServiceDescriptionFormatExtension
    {
        public string SoapAction { get; set; }
        public SoapBindingStyle Style { get; set; }
    }
}

#endif
