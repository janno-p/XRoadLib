using System.IO;
using XRoadLib.Attributes;

namespace Calculator.Contract.Types
{
    [XRoadSerializable]
    public class FileTransferRequest
    {
        [XRoadXmlElement(UseXop = false)]
        public Stream Input { get; set; }
    }
}