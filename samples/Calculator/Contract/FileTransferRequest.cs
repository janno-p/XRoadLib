using System.IO;
using XRoadLib.Attributes;

namespace Calculator.Contract
{
    [XRoadOperation("FileTransfer")]
    public class FileTransferRequest : ICalculatorRequest<Stream>
    {
        [XRoadXmlElement(UseXop = false)]
        public Stream Input { get; set; }
    }
}