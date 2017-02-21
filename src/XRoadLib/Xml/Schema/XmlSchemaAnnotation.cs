#if NETSTANDARD1_6_1

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAnnotation : XmlSchemaObject
    {
        protected override string ElementName { get; } = "annotation";

        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Items.ForEach(x => x.Write(writer));
        }
    }
}

#endif