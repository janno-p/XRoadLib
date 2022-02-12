namespace XRoadLib.Serialization;

internal class XRoadFault : IXRoadFault
{
    public string FaultCode { get; set; }
    public string FaultString { get; set; }
}