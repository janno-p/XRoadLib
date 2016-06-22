#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class MessagePart : NamedItem
    {
        public XmlQualifiedName Element { get; set; }
        public XmlQualifiedName Type { get; set; }
    }
}

#endif