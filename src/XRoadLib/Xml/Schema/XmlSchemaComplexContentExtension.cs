#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContentExtension : XmlSchemaContent
    {
        public XmlQualifiedName BaseTypeName { get; set; }
        public XmlSchemaParticle Particle { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif