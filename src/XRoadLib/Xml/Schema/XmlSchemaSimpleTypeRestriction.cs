#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
    {
        public XmlQualifiedName BaseTypeName { get; set; }
        public IList<XmlSchemaFacet> Facets { get; } = new List<XmlSchemaFacet>();
    }
}

#endif