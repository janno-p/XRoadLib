namespace XRoadLib.Soap
{
    public sealed class ClientFaultCode : FaultCode
    {
        public static ClientFaultCode InvalidQuery => new ClientFaultCode(nameof(InvalidQuery));
        public static ClientFaultCode ParameterRequired => new ClientFaultCode(nameof(ParameterRequired));
        public static ClientFaultCode UnexpectedElement => new ClientFaultCode(nameof(UnexpectedElement));
        public static ClientFaultCode UnexpectedValue => new ClientFaultCode(nameof(UnexpectedValue));
        public static ClientFaultCode UnknownOperation => new ClientFaultCode(nameof(UnknownOperation));
        public static ClientFaultCode UnknownProperty => new ClientFaultCode(nameof(UnknownProperty));
        public static ClientFaultCode UnknownType => new ClientFaultCode(nameof(UnknownType));
        public static ClientFaultCode UnsupportedContentTransferEncoding => new ClientFaultCode(nameof(UnsupportedContentTransferEncoding));

        public ClientFaultCode(string value = null)
            : base(FaultCodeType.Client, value)
        { }
    }
}