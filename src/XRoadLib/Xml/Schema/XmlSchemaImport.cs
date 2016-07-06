#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaImport : XmlSchemaExternal
    {
        public string Namespace { get; set; }

        internal override void Write(XmlWriter writer)
        {
            WriteStartElement(writer, "import");
            WriteAttributes(writer);
            writer.WriteEndElement();
        }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(Namespace))
                writer.WriteAttributeString("namespace", Namespace);
        }
    }
}

#endif
