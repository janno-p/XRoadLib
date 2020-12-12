using System.IO;
using XRoadLib.Serialization;

namespace Calculator.Contract
{
    public class FileCalculationRequest : XRoadSerializable
    {
        public Stream InputFile { get; set; }
    }
}