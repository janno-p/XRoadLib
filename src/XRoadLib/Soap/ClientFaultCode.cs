namespace XRoadLib.Soap
{
    public sealed class ClientFaultCode : FaultCode
    {
        public ClientFaultCode(string value = null)
            : base(FaultCodeType.Client, value)
        { }
    }
}