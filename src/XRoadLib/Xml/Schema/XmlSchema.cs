#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchema : XmlSchemaObject
    {
        public List<XmlSchemaExternal> Includes { get; } = new List<XmlSchemaExternal>();
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
        public string TargetNamespace { get; set; }

        internal override void Write(XmlWriter writer)
        {
            WriteStartElement(writer, "schema");

            WriteAttributes(writer);

            Includes.ForEach(x => x.Write(writer));
            Items.ForEach(x => x.Write(writer));

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
