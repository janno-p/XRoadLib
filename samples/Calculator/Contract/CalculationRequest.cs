using System.Xml.Serialization;
using XRoadLib;
using XRoadLib.Serialization;

namespace Calculator.Contract
{
    public class CalculationRequest : XRoadSerializable, IXRoadRequest<int>
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }

        [XmlElement(Order = 3)]
        public Operation Operation { get; set; }
    }
}