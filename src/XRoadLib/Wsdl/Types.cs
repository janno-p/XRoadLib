using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XRoadLib.Wsdl
{
    public class Types : DocumentableItem
    {
        protected override string ElementName { get; } = "types";

        public List<XmlSchema> Schemas { get; } = new List<XmlSchema>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Schemas.ForEach(x => x.Write(writer));
        }
    }
}