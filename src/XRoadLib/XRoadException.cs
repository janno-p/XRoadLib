using System.Runtime.Serialization;
using XRoadLib.Soap;

namespace XRoadLib;

[Serializable]
public abstract class XRoadException : Exception
{
    public FaultCode FaultCode { get; }

    protected XRoadException(FaultCode faultCode, string message)
        : base(message)
    {
        FaultCode = faultCode;
    }

    protected XRoadException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        FaultCode = default!;
    }
}