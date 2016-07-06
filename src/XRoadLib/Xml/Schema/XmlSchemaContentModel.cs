#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaContentModel : XmlSchemaAnnotated
    {
        public XmlSchemaContent Content { get; set; }
    }
}

#endif