#if NETSTANDARD1_6_1

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaChoice : XmlSchemaGroupBase
    {
        protected override string ElementName { get; } = "choice";
    }
}

#endif