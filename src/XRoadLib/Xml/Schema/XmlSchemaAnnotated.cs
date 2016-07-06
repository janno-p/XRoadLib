#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaAnnotated : XmlSchemaObject
    {
        public XmlSchemaAnnotation Annotation { get; set; }
        public List<XmlAttribute> UnhandledAttributes { get; } = new List<XmlAttribute>();
    }
}

#endif