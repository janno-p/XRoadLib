namespace XRoadLib.Soap;

public abstract class FaultCode
{
    protected enum FaultCodeType
    {
        Server,
        Client,
        [UsedImplicitly] VersionMismatch,
        [UsedImplicitly] MustUnderstand
    }

    public string Value { get; }

    protected FaultCode(FaultCodeType type, string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? type.ToString() : $"{type}.{value}";
    }
}