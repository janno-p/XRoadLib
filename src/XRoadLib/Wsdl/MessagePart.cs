using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Wsdl
{
    public class MessagePart : NamedItem
    {
        protected override string ElementName { get; } = "part";

        public XmlQualifiedName Element { get; set; }
        public XmlQualifiedName Type { get; set; }

        protected override async Task WriteAttributesAsync(XmlWriter writer)
        {
            await base.WriteAttributesAsync(writer).ConfigureAwait(false);
            await writer.WriteQualifiedAttributeAsync("element", Element).ConfigureAwait(false);
            await writer.WriteQualifiedAttributeAsync("type", Type).ConfigureAwait(false);
        }
    }
}