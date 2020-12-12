using System.IO;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    public interface IFileCalculation
    {
        [XRoadService("FileCalculation")]
        [XRoadTitle("en", "File calculations")]
        [XRoadNotes("en", "Calculates values for each formula given in input file.")]
        Stream Sum(FileCalculationRequest request);
    }
}