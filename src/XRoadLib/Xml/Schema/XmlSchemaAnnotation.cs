#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAnnotation : XmlSchemaObject
    {
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif