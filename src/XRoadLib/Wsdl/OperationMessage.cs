using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Wsdl
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