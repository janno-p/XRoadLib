#if NETSTANDARD1_6_1

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaEnumerationFacet : XmlSchemaFacet
    {
        protected override string ElementName { get; } = "enumeration";
    }
}

#endif