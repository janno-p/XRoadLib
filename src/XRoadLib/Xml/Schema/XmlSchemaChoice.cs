#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaChoice : XmlSchemaGroupBase
    {
        protected override string ElementName { get; } = "choice";
    }
}

#endif