#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class MimeContentBinding : ServiceDescriptionFormatExtension
    {
        public string Part { get; set; }
        public string Type { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif