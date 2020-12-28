using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace Calculator.Contract.Types
{
    [XRoadSerializable]
    public class AddRequest
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }
    }
}