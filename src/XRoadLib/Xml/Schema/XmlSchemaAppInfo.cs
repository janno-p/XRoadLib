#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAppInfo : XmlSchemaObject
    {
        public XmlNode[] Markup { get; set; }
    }
}

#endif