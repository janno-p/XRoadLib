using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Operation : NamedItem
    {
        protected override string ElementName { get; } = "operation";

        [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
        public List<OperationMessage> Messages { get; } = new();

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            foreach (var message in Messages)
                await message.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}