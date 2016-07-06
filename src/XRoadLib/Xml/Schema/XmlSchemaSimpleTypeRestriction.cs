#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
    {
        public XmlQualifiedName BaseTypeName { get; set; }
        public List<XmlSchemaFacet> Facets { get; } = new List<XmlSchemaFacet>();

        internal override void Write(XmlWriter writer)
        {

        }
    }
}

#endif