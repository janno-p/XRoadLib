using System.Runtime.Serialization;

namespace XRoadLib.Soap;

[Serializable]
public class SoapFaultException : Exception
{
    [UsedImplicitly]
    public ISoapFault Fault { get; }

    public SoapFaultException(ISoapFault fault)
        : base(fault?.FaultString)
    {
        Fault = fault ?? throw new ArgumentNullException(nameof(fault));
    }

    protected SoapFaultException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}