#if NETSTANDARD1_6_1

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAppInfo : XmlSchemaObject
    {
        protected override string ElementName { get; } = "appinfo";

        public List<XmlNode> Markup { get; } = new List<XmlNode>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Markup.ForEach(x => x.WriteTo(writer));
        }
    }
}

#endif