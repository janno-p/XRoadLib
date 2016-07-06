#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaElement : XmlSchemaParticle
    {
        public bool IsNillable { get; set; }
        public string Name { get; set; }
        public XmlSchemaType SchemaType { get; set; }
        public XmlQualifiedName SchemaTypeName { get; set; }

        internal override void Write(XmlWriter writer)
        {
            WriteStartElement(writer, "element");
            WriteAttributes(writer);
            base.Write(writer);
            SchemaType?.Write(writer);
            writer.WriteEndElement();
        }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteAttributeString("name", Name);

            if (IsNillable)
                writer.WriteAttributeString("nillable", XmlConvert.ToString(IsNillable));

            if (SchemaType == null && !SchemaTypeName.IsEmpty)
            {
                writer.WriteStartAttribute("type");
                writer.WriteQualifiedName(SchemaTypeName.Name, SchemaTypeName.Namespace);
                writer.WriteEndAttribute();
            }
        }
    }
}

#endif