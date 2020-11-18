using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class PortType : NamedItem
    {
        protected override string ElementName { get; } = "portType";

        public List<Operation> Operations { get; } = new List<Operation>();

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            foreach (var operation in Operations)
                await operation.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}