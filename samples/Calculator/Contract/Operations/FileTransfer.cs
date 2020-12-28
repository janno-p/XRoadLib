using System.IO;
using Calculator.Contract.Types;
using XRoadLib.Attributes;

namespace Calculator.Contract.Operations
{
    [XRoadOperation]
    public class FileTransfer : CalculatorOperation<FileTransferRequest, Stream>
    { }
}