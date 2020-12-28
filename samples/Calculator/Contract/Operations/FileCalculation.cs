using System.IO;
using Calculator.Contract.Types;
using XRoadLib.Attributes;

namespace Calculator.Contract.Operations
{
    [XRoadOperation("FileCalculation")]
    [XRoadTitle("en", "File calculations")]
    [XRoadNotes("en", "Calculates values for each formula given in input file.")]
    public class FileCalculation : CalculatorOperation<FileCalculationRequest, Stream>
    { }
}