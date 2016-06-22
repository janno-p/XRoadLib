#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContentRestriction : XmlSchemaContent
    {
        public XmlQualifiedName BaseTypeName { get; set; }
        public XmlSchemaParticle Particle { get; set; }
        public IList<XmlSchemaObject> Attributes { get; } = new List<XmlSchemaObject>();
    }
}

#endif