#if NETSTANDARD1_6_1

using System.Xml;
using XRoadLib.Extensions;

namespace System.Web.Services.Description
{
    public abstract class OperationMessage : NamedItem
    {
        public XmlQualifiedName Message { get; set; }

        protected override void WriteAttributes(XmlWriter writer)
        {
            base.WriteAttributes(writer);
            writer.WriteQualifiedAttribute("message", Message);
        }
    }
}

#endif