#if NETSTANDARD1_5

using System.Xml;
using XRoadLib.Extensions;

namespace System.Web.Services.Description
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

#endif