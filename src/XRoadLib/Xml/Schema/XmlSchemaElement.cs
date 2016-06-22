#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaElement : XmlSchemaParticle
    {
        public bool IsNillable { get; set; }
        public string Name { get; set; }
        public XmlSchemaType SchemaType { get; set; }
        public XmlQualifiedName SchemaTypeName { get; set; }
    }
}

#endif