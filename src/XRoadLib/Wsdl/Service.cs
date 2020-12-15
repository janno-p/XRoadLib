using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Service : NamedItem
    {
        protected override string ElementName { get; } = "service";

        public List<Port> Ports { get; } = new();

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            foreach (var port in Ports)
                await port.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}