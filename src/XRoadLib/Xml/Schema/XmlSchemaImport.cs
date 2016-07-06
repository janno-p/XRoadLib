#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaImport : XmlSchemaExternal
    {
        public string Namespace { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif
