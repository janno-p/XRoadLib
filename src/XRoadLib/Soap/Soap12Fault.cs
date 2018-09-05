using System.Collections.Generic;

namespace XRoadLib.Soap
{
    public class Soap12Fault : ISoap12Fault
    {
        public Soap12FaultCode Code { get; set; }
        public IList<Soap12FaultReasonText> Reason { get; set; }
        public string Node { get; set; }
        public string Role { get; set; }
        public string Detail { get; set; }
    }
}