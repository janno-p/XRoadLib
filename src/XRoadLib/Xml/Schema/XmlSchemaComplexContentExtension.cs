#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContentExtension : XmlSchemaContent
    {
        protected override string ElementName { get; } = "extension";

        public XmlQualifiedName BaseTypeName { get; set; }
        public XmlSchemaParticle Particle { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!BaseTypeName.IsEmpty)
            {
                writer.WriteStartAttribute("base");
                writer.WriteQualifiedName(BaseTypeName.Name, BaseTypeName.Namespace);
                writer.WriteEndAttribute();
            }
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Particle?.Write(writer);
        }
    }
}

#endif