using System.IO;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadSerializable]
    [XRoadOperation("FileCalculation")]
    [XRoadTitle("en", "File calculations", Target = DocumentationTarget.Operation)]
    [XRoadNotes("en", "Calculates values for each formula given in input file.", Target = DocumentationTarget.Operation)]
    public class FileCalculationRequest : ICalculatorRequest<Stream>
    {
        public Stream InputFile { get; set; }
    }
}