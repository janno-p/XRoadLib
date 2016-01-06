namespace XRoadLib.Soap
{
    public sealed class ServerFaultCode : FaultCode
    {
        public ServerFaultCode(string value = null)
            : base(FaultCodeType.Server, value)
        { }

        public static FaultCode InternalError => new ServerFaultCode("InternalError");
    }
}