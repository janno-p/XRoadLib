#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaParticle : XmlSchemaAnnotated
    {
        public decimal MinOccurs { get; set; } = 1M;
        public decimal MaxOccurs { get; set; } = 1M;
        public string MaxOccursString { get; set; }
    }
}

#endif
