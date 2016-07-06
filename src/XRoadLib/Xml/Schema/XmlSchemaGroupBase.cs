#if NETSTANDARD1_5

using System.Collections.Generic;

namespace XRoadLib.Xml.Schema
{
    public abstract class XmlSchemaGroupBase : XmlSchemaParticle
    {
        public List<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
    }
}

#endif