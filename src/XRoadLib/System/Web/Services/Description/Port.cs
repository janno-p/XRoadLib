#if NETSTANDARD1_5

using System.Xml;

namespace System.Web.Services.Description
{
    public class Port : NamedItem
    {
        public XmlQualifiedName Binding { get; set; }
    }
}

#endif