using System.Xml;

namespace XRoadLib.Wsdl
{
    public class OperationBinding : NamedItem
    {
        protected override string ElementName { get; } = "operation";

        public InputBinding Input { get; set; }
        public OutputBinding Output { get; set; }

        protected override void WriteElements(XmlWriter writer)
        {
            base.WriteElements(writer);
            Input?.Write(writer);
            Output?.Write(writer);
        }
    }
}