#if NETSTANDARD1_5

namespace System.Web.Services.Description
{
    public class XRoadAddressBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Producer { get; set; }

        public XRoadAddressBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }
    }
}

#endif