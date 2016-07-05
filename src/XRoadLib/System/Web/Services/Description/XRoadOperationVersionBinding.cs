#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class XRoadOperationVersionBinding : ServiceDescriptionFormatExtension
    {
        public string Prefix { get; }
        public string Namespace { get; }

        public string Version { get; set; }

        public XRoadOperationVersionBinding(string prefix, string ns)
        {
            Prefix = prefix;
            Namespace = ns;
        }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif