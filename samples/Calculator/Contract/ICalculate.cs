using System.Threading.Tasks;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    public interface ICalculate
    {
        [XRoadService(nameof(Calculate))]
        [XRoadTitle("en", "Calculation service")]
        [XRoadNotes("en", "Performs specified operation on two user provided integers and returns the result.")]
        int Calculate(CalculationRequest request);

        [XRoadService(nameof(CalculateAsync))]
        [XRoadTitle("en", "Calculation service")]
        [XRoadNotes("en", "Performs specified operation on two user provided integers and returns the result.")]
        Task<int> CalculateAsync(CalculationRequest request);
    }
}