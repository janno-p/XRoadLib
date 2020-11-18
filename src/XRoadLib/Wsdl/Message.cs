using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace XRoadLib.Wsdl
{
    public class Message : NamedItem
    {
        protected override string ElementName { get; } = "message";

        public List<MessagePart> Parts { get; } = new List<MessagePart>();

        protected override async Task WriteElementsAsync(XmlWriter writer)
        {
            await base.WriteElementsAsync(writer).ConfigureAwait(false);

            foreach (var part in Parts)
                await part.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}