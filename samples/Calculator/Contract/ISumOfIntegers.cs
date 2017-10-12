using XRoadLib.Attributes;

namespace Calculator.Contract
{
    public interface ISumOfIntegers
    {
        [XRoadService("SumOfIntegers")]
        [XRoadTitle("en", "Sum of integers")]
        [XRoadNotes("en", "Calculates sum of two user provided integers and returns the result.")]
        int Sum(AddRequest request);
    }
}