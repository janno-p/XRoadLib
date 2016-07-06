#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAppInfo : XmlSchemaObject
    {
        public List<XmlNode> Markup { get; } = new List<XmlNode>();

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif