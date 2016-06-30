#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
{
    public class DocumentableItem
    {
        public XmlElement DocumentationElement { get; set; }
        public List<XmlAttribute> ExtensibleAttributes { get; } = new List<XmlAttribute>();
        public List<ServiceDescriptionFormatExtension> Extensions { get; } = new List<ServiceDescriptionFormatExtension>();
        public Dictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();
    }
}

#endif