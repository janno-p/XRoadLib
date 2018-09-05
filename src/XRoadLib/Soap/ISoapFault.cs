namespace XRoadLib.Soap
{
    public interface ISoapFault : IFault
    {
        string FaultCode { get; set; }
        string FaultString { get; set; }
        string FaultActor { get; set; }
        string Details { get; set; }
    }
}