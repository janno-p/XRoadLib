using System.Xml.Serialization;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadSerializable]
    [XRoadOperation("SumOfIntegers")]
    [XRoadTitle("en", "Sum of integers", Target = DocumentationTarget.Operation)]
    [XRoadNotes("en", "Calculates sum of two user provided integers and returns the result.", Target = DocumentationTarget.Operation)]
    public class AddRequest : ICalculatorRequest<int>
    {
        [XmlElement(Order = 1)]
        public int X { get; set; }

        [XmlElement(Order = 2)]
        public int Y { get; set; }
    }
}