namespace XRoadLib.Soap;

public sealed class ClientFaultCode : FaultCode
{
    public static ClientFaultCode InvalidQuery => new(nameof(InvalidQuery));
    public static ClientFaultCode ParameterRequired => new(nameof(ParameterRequired));
    public static ClientFaultCode UnexpectedElement => new(nameof(UnexpectedElement));
    public static ClientFaultCode UnexpectedValue => new(nameof(UnexpectedValue));
    public static ClientFaultCode UnknownOperation => new(nameof(UnknownOperation));
    public static ClientFaultCode UnknownProperty => new(nameof(UnknownProperty));
    public static ClientFaultCode UnknownType => new(nameof(UnknownType));
    public static ClientFaultCode UnsupportedContentTransferEncoding => new(nameof(UnsupportedContentTransferEncoding));

    public ClientFaultCode(string? value = null)
        : base(FaultCodeType.Client, value)
    { }
}