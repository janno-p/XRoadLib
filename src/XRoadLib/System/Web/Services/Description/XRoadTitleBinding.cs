#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class XRoadTitleBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Text { get; set; }
        public string Language { get; set; }

        public XRoadTitleBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }
    }
}

#endif