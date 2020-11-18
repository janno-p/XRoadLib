using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Operation : NamedItem
    {
        protected override string ElementName { get; } = "operation";

        public List<OperationMessage> Messages { get; } = new List<OperationMessage>();

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            foreach (var message in Messages)
                await message.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}