using XRoadLib.Attributes;

namespace Calculator.Contract
{
    public interface ISumOfIntegers
    {
        [XRoadService("SumOfIntegers")]
        int Sum(AddRequest request);
    }
}