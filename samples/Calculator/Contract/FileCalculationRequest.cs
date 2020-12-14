using System.IO;
using XRoadLib;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadSerializable]
    public class FileCalculationRequest : IXRoadRequest<Stream>
    {
        public Stream InputFile { get; set; }
    }
}