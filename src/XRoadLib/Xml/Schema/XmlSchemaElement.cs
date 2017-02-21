#if NETSTANDARD1_6_1

using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaElement : XmlSchemaParticle
    {
        protected override string ElementName { get; } = "element";

        public bool IsNillable { get; set; }
        public string Name { get; set; }
        public XmlSchemaType SchemaType { get; set; }
        public XmlQualifiedName SchemaTypeName { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteAttributeString("name", Name);

            if (IsNillable)
                writer.WriteAttributeString("nillable", XmlConvert.ToString(IsNillable));

            if (SchemaType == null)
                writer.WriteQualifiedAttribute("type", SchemaTypeName);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            SchemaType?.Write(writer);
        }
    }
}

#endif