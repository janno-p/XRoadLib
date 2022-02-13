namespace XRoadLib.Soap;

public class Soap12FaultCode
{
    public Soap12FaultCodeValue Value { get; set; } = Soap12FaultCodeValue.None;
    public Soap12FaultSubcode Subcode { get; set; }
}