using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class OperationBinding : NamedItem
    {
        protected override string ElementName { get; } = "operation";

        public InputBinding Input { get; set; }
        public OutputBinding Output { get; set; }

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            if (Input != null)
                await Input.WriteAsync(writer).ConfigureAwait(false);

            if (Output != null)
                await Output.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}