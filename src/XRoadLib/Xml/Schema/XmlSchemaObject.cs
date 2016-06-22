#if NETSTANDARD1_5

using System.Collections.Generic;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaObject
    {
        public IDictionary<string, string> Namespaces { get; } = new Dictionary<string, string>();
    }
}

#endif
