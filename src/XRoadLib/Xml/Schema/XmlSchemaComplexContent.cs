#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContent : XmlSchemaContentModel
    {
        protected override string ElementName { get; } = "complexContent";
    }
}

#endif