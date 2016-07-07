#if NETSTANDARD1_5

using System.Xml;
using XRoadLib.Extensions;

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
            writer.WriteQualifiedAttribute("base", BaseTypeName);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Particle?.Write(writer);
        }
    }
}

#endif