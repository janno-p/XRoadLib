namespace XRoadLib.Soap;

public sealed class ServerFaultCode : FaultCode
{
    public static FaultCode InternalError => new ServerFaultCode(nameof(InternalError));

    public ServerFaultCode(string value = null)
        : base(FaultCodeType.Server, value)
    { }
}