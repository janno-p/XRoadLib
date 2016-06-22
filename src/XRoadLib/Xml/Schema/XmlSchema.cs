#if NETSTANDARD1_5

using System.Collections.Generic;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchema : XmlSchemaObject
    {
        public IList<XmlSchemaObject> Includes { get; } = new List<XmlSchemaObject>();
        public IList<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
        public XmlSchemaType SchemaType { get; set; }
        public string TargetNamespace { get; set; }
    }
}

#endif
