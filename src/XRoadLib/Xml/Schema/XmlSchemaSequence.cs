#if NETSTANDARD1_6_1

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSequence : XmlSchemaGroupBase
    {
        protected override string ElementName { get; } = "sequence";
    }
}

#endif