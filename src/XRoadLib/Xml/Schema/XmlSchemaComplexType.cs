#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexType : XmlSchemaType
    {
        public XmlSchemaContentModel ContentModel { get; set; }
        public bool IsAbstract { get; set; }
        public XmlSchemaParticle Particle { get; set; }
    }
}

#endif