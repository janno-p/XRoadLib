using Calculator.Contract.Types;
using XRoadLib.Attributes;

namespace Calculator.Contract.Operations
{
    [XRoadOperation]
    [XRoadTitle("en", "Sum of integers")]
    [XRoadNotes("en", "Calculates sum of two user provided integers and returns the result.")]
    public class SumOfIntegers : CalculatorOperation<AddRequest, int>
    { }
}