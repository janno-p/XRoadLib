#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaType : XmlSchemaAnnotated
    {
        public string Name { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Name))
                writer.WriteAttributeString("name", Name);
        }
    }
}

#endif