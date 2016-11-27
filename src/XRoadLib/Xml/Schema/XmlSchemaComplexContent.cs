#if NETSTANDARD1_6

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaComplexContent : XmlSchemaContentModel
    {
        protected override string ElementName { get; } = "complexContent";
    }
}

#endif