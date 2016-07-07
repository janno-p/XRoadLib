#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaEnumerationFacet : XmlSchemaFacet
    {
        protected override string ElementName { get; } = "enumeration";
    }
}

#endif