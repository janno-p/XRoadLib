using Calculator.Contract.Types;
using XRoadLib.Attributes;

namespace Calculator.Contract.Operations
{
    [XRoadOperation]
    [XRoadTitle("en", "Calculation service")]
    [XRoadNotes("en", "Performs specified operation on two user provided integers and returns the result.")]
    public class Calculate : CalculatorOperation<CalculationRequest, int>
    { }
}