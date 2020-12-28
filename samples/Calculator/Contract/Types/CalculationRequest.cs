using System.Xml.Serialization;
using MediatR;
using XRoadLib.Attributes;

namespace Calculator.Contract.Types
{
    [XRoadSerializable]
    public class CalculationRequest
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }

        [XmlElement(Order = 3)]
        public Operator Operator { get; set; }
    }
}