#if NETSTANDARD1_5

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSequence : XmlSchemaGroupBase
    {
        protected override string ElementName { get; } = "sequence";
    }
}

#endif