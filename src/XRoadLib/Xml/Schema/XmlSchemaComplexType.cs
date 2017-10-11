#if NETSTANDARD1_6

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexType : XmlSchemaType
    {
        protected override string ElementName { get; } = "complexType";

        public XmlSchemaContentModel ContentModel { get; set; }
        public bool IsAbstract { get; set; }
        public XmlSchemaParticle Particle { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (IsAbstract)
                writer.WriteAttributeString("abstract", XmlConvert.ToString(IsAbstract));
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);

            if (ContentModel != null) ContentModel.Write(writer);
            else Particle?.Write(writer);
        }
    }
}

#endif