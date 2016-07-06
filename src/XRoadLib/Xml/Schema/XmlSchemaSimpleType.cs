#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSimpleType : XmlSchemaType
    {
        public XmlSchemaSimpleTypeContent Content { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif