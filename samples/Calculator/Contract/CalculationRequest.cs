using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadSerializable]
    [XRoadOperation("Calculate")]
    [XRoadTitle("en", "Calculation service", Target = DocumentationTarget.Operation)]
    [XRoadNotes("en", "Performs specified operation on two user provided integers and returns the result.", Target = DocumentationTarget.Operation)]
    public class CalculationRequest : ICalculatorRequest<int>
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }

        [XmlElement(Order = 3)]
        public Operation Operation { get; set; }
    }
}