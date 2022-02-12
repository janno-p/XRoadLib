namespace XRoadLib.Soap;

public interface ISoapFault : IFault
{
    [UsedImplicitly]
    string FaultCode { get; set; }

    [UsedImplicitly]
    string FaultString { get; set; }

    [UsedImplicitly]
    string FaultActor { get; set; }

    [UsedImplicitly]
    string Details { get; set; }
}