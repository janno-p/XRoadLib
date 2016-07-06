#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaFacet : XmlSchemaAnnotated
    {
        public string Value { get; set; }
    }
}

#endif