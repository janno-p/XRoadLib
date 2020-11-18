using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Wsdl
{
    public class Port : NamedItem
    {
        protected override string ElementName { get; } = "port";

        public XmlQualifiedName Binding { get; set; }

        protected override async Task WriteAttributesAsync(XmlWriter writer)
        {
            await base.WriteAttributesAsync(writer).ConfigureAwait(false);
            await writer.WriteQualifiedAttributeAsync("binding", Binding).ConfigureAwait(false);
        }
    }
}