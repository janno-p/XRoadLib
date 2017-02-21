#if NETSTANDARD1_6_1

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchema : XmlSchemaObject
    {
        protected override string ElementName { get; } = "schema";

        public List<XmlSchemaExternal> Includes { get; } = new List<XmlSchemaExternal>();
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
        public string TargetNamespace { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            if (!string.IsNullOrWhiteSpace(TargetNamespace))
                writer.WriteAttributeString("targetNamespace", TargetNamespace);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Includes.ForEach(x => x.Write(writer));
            Items.ForEach(x => x.Write(writer));
        }
    }
}

#endif
