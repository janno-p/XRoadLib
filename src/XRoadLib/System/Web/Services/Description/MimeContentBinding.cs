#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class MimeContentBinding : ServiceDescriptionFormatExtension
    {
        public string Part { get; set; }
        public string Type { get; set; }
    }
}

#endif