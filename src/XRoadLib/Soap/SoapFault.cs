namespace XRoadLib.Soap
{
    internal class SoapFault : ISoapFault
    {
        public string FaultCode { get; set; }
        public string FaultString { get; set; }
        public string FaultActor { get; set; }
        public string Details { get; set; }
    }
}