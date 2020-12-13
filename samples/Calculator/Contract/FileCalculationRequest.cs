using System.IO;
using XRoadLib;
using XRoadLib.Serialization;

namespace Calculator.Contract
{
    public class FileCalculationRequest : XRoadSerializable, IXRoadRequest<Stream>
    {
        public Stream InputFile { get; set; }
    }
}