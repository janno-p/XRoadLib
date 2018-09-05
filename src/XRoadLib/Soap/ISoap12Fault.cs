using System.Collections.Generic;

namespace XRoadLib.Soap
{
    public interface ISoap12Fault : IFault
    {
        Soap12FaultCode Code { get; set; }
        IList<Soap12FaultReasonText> Reason { get; set; }
        string Node { get; set; }
        string Role { get; set; }
        string Detail { get; set; }
    }
}