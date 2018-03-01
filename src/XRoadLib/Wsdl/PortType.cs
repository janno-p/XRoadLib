using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class PortType : NamedItem
    {
        protected override string ElementName { get; } = "portType";

        public List<Operation> Operations { get; } = new List<Operation>();

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Operations.ForEach(x => x.Write(writer));
        }
    }
}