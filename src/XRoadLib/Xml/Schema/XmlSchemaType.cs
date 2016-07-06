#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaType : XmlSchemaAnnotated
    {
        public string Name { get; set; }
    }
}

#endif