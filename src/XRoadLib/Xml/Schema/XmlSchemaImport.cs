#if NETSTANDARD1_6

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaImport : XmlSchemaExternal
    {
        protected override string ElementName { get; } = "import";

        public string Namespace { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Namespace))
                writer.WriteAttributeString("namespace", Namespace);
        }
    }
}

#endif
