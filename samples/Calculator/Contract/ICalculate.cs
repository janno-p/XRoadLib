using System.Diagnostics.CodeAnalysis;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    public interface ICalculate
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [XRoadService("Calculate")]
        [XRoadTitle("en", "Calculation service")]
        [XRoadNotes("en", "Performs specified operation on two user provided integers and returns the result.")]
        int Calculate(CalculationRequest request);
    }
}