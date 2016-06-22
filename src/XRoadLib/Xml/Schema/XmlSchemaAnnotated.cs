#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAnnotated : XmlSchemaObject
    {
        public XmlSchemaAnnotation Annotation { get; set; }
        public XmlAttribute[] UnhandledAttributes { get; set; }
    }
}

#endif