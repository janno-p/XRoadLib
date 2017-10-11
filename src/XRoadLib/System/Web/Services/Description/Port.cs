#if NETSTANDARD1_6

using System.Xml;
using XRoadLib.Extensions;

namespace System.Web.Services.Description
{
    public class Port : NamedItem
    {
        protected override string ElementName { get; } = "port";

        public XmlQualifiedName Binding { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("binding", Binding);
        }
    }
}

#endif