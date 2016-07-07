#if NETSTANDARD1_5

using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAttribute : XmlSchemaAnnotated
    {
        protected override string ElementName { get; } = "attribute";

        public XmlQualifiedName RefName { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("ref", RefName);
        }
    }
}

#endif