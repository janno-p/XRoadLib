#if NETSTANDARD1_5

using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaAttribute : XmlSchemaAnnotated
    {
        public XmlQualifiedName RefName { get; set; }

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif