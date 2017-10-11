#if NETSTANDARD1_6

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaFacet : XmlSchemaAnnotated
    {
        public string Value { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Value))
                writer.WriteAttributeString("value", Value);
        }
    }
}

#endif