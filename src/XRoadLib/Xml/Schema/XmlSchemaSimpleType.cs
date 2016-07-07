#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSimpleType : XmlSchemaType
    {
        protected override string ElementName { get; } = "simpleType";

        public XmlSchemaSimpleTypeContent Content { get; set; }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Content?.Write(writer);
        }
    }
}

#endif