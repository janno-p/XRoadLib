#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAttribute : XmlSchemaAnnotated
    {
        protected override string ElementName { get; } = "attribute";

        public XmlQualifiedName RefName { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!RefName.IsEmpty)
            {
                writer.WriteStartAttribute("ref");
                writer.WriteQualifiedName(RefName.Name, RefName.Namespace);
                writer.WriteEndAttribute();
            }
        }
    }
}

#endif