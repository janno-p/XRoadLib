#if NETSTANDARD1_5

using System.Collections.Generic;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaGroupBase : XmlSchemaParticle
    {
        public IList<XmlSchemaObject> Items { get; } = new List<XmlSchemaObject>();
    }
}

#endif