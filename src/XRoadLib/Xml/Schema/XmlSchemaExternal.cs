#if NETSTANDARD1_6

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaExternal : XmlSchemaObject
    {
        public string SchemaLocation { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);

            if (!string.IsNullOrWhiteSpace(SchemaLocation))
                writer.WriteAttributeString("schemaLocation", SchemaLocation);
        }
    }
}

#endif
