using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class UnknownOperationException : ContractViolationException
{
    public UnknownOperationException(string message)
        : base(ClientFaultCode.UnknownOperation, message)
    { }

    protected UnknownOperationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}