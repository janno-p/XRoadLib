#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexType : XmlSchemaType
    {
        public XmlSchemaContentModel ContentModel { get; set; }
        public bool IsAbstract { get; set; }
        public XmlSchemaParticle Particle { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif