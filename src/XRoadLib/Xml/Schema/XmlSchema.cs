#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchema : XmlSchemaObject
    {
        public List<XmlSchemaObject> Includes { get; } = new List<XmlSchemaObject>();
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
        public string TargetNamespace { get; set; }

        internal override void Write(XmlWriter writer)
        {
            WriteStartElement(writer, "schema");

            WriteAttributes(writer);

            writer.WriteEndElement();
        }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(TargetNamespace))
                writer.WriteAttributeString("targetNamespace", TargetNamespace);
        }
    }
}

#endif
