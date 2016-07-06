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
            WriteStartElement(writer, "complexType");
            WriteAttributes(writer);
            base.Write(writer);

            if (ContentModel != null) ContentModel.Write(writer);
            else Particle?.Write(writer);

            writer.WriteEndElement();
        }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (IsAbstract)
                writer.WriteAttributeString("abstract", XmlConvert.ToString(IsAbstract));
        }
    }
}

#endif