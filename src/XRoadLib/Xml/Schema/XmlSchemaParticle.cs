#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaParticle : XmlSchemaAnnotated
    {
        public decimal MinOccurs { get; set; } = 1M;
        public decimal MaxOccurs { get; set; } = 1M;
        public string MaxOccursString { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (MinOccurs != 1M)
                writer.WriteAttributeString("minOccurs", XmlConvert.ToString(MinOccurs));

            if (!string.IsNullOrWhiteSpace(MaxOccursString))
                writer.WriteAttributeString("maxOccurs", MaxOccursString);
            else if (MaxOccurs != 1M)
                writer.WriteAttributeString("maxOccurs", XmlConvert.ToString(MaxOccurs));
        }
    }
}

#endif
