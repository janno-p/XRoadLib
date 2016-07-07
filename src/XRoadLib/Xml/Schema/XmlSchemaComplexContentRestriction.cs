#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContentRestriction : XmlSchemaContent
    {
        protected override string ElementName { get; } = "restriction";

        public XmlQualifiedName BaseTypeName { get; set; }
        public XmlSchemaParticle Particle { get; set; }
        public List<XmlSchemaObject> Attributes { get; } = new List<XmlSchemaObject>();

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("base", BaseTypeName);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Particle?.Write(writer);
            Attributes.ForEach(x => x.Write(writer));
        }
    }
}

#endif