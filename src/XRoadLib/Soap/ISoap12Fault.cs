using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Soap
{
    public interface ISoap12Fault : IFault
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        Soap12FaultCode Code { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        IList<Soap12FaultReasonText> Reason { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string Node { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string Role { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string Detail { get; set; }
    }
}