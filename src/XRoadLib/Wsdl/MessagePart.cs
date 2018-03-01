using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Wsdl
{
    public class MessagePart : NamedItem
    {
        protected override string ElementName { get; } = "part";

        public XmlQualifiedName Element { get; set; }
        public XmlQualifiedName Type { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("element", Element);
            writer.WriteQualifiedAttribute("type", Type);
        }
    }
}