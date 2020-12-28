using System.IO;
using XRoadLib.Attributes;

namespace Calculator.Contract.Types
{
    [XRoadSerializable]
    public class FileCalculationRequest
    {
        public Stream InputFile { get; set; }
    }
}