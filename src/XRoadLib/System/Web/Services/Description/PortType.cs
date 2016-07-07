#if NETSTANDARD1_5

using System.Collections.Generic;
using System.Xml;

namespace System.Web.Services.Description
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

#endif