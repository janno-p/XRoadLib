using System.Runtime.Serialization;

namespace XRoadLib.Serialization;

[Serializable]
public class XRoadFaultException : Exception, IXRoadFault
{
    public string FaultCode { get; }
    public string FaultString { get; }

    public XRoadFaultException(IXRoadFault xRoadFault, Exception innerException = null)
        : base(xRoadFault?.FaultString, innerException)
    {
        if (xRoadFault == null)
            throw new ArgumentNullException(nameof(xRoadFault));

        FaultCode = xRoadFault.FaultCode;
        FaultString = xRoadFault.FaultString;
    }

    protected XRoadFaultException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
}