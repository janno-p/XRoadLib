#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaAnnotated : XmlSchemaObject
    {
        public XmlSchemaAnnotation Annotation { get; set; }
        public List<XmlAttribute> UnhandledAttributes { get; } = new List<XmlAttribute>();

        internal override void Write(XmlWriter writer)
        {
            Annotation?.Write(writer);
        }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            UnhandledAttributes.ForEach(a => a.WriteTo(writer));
        }
    }
}

#endif