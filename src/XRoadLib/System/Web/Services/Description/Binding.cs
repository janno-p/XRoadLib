#if NETSTANDARD2_0

using System.Collections.Generic;
using System.Xml;
using XRoadLib.Extensions;

namespace System.Web.Services.Description
{
    public class Binding : NamedItem
    {
        protected override string ElementName { get; } = "binding";

        public List<OperationBinding> Operations { get; } = new List<OperationBinding>();
        public XmlQualifiedName Type { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("type", Type);
        }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Operations.ForEach(x => x.Write(writer));
        }
    }
}

#endif