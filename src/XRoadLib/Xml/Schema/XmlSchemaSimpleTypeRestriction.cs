#if NETSTANDARD1_6_1

using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Xml.Schema
{
    public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
    {
        protected override string ElementName { get; } = "restriction";

        public XmlQualifiedName BaseTypeName { get; set; }
        public List<XmlSchemaFacet> Facets { get; } = new List<XmlSchemaFacet>();

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("base", BaseTypeName);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Facets.ForEach(x => x.Write(writer));
        }
    }
}

#endif