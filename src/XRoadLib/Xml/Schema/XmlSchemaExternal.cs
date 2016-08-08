using System.Xml;

#if NETSTANDARD1_5

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
