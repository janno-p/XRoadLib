using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Schema;

[Serializable]
public class ContractViolationException : XRoadException
{
    [UsedImplicitly]
    public ContractViolationException(FaultCode faultCode, string message)
        : base(faultCode, message)
    { }

    protected ContractViolationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}