#if NETSTANDARD1_6

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaAnnotated : XmlSchemaObject
    {
        public XmlSchemaAnnotation Annotation { get; set; }
        public List<XmlAttribute> UnhandledAttributes { get; } = new List<XmlAttribute>();

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            UnhandledAttributes.ForEach(a => a.WriteTo(writer));
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Annotation?.Write(writer);
        }
    }
}

#endif