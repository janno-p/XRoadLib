#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaGroupBase : XmlSchemaParticle
    {
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();

        internal override void Write(XmlWriter writer)
        {
            base.Write(writer);
            Items.ForEach(x => x.Write(writer));
        }
    }
}

#endif