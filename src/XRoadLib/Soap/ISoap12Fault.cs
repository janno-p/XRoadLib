namespace XRoadLib.Soap;

public interface ISoap12Fault : IFault
{
    [UsedImplicitly]
    Soap12FaultCode Code { get; set; }

    [UsedImplicitly]
    IList<Soap12FaultReasonText> Reason { get; set; }

    [UsedImplicitly]
    string Node { get; set; }

    [UsedImplicitly]
    string Role { get; set; }

    [UsedImplicitly]
    string Detail { get; set; }
}