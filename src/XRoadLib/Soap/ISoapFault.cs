using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Soap
{
    public interface ISoapFault : IFault
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string FaultCode { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string FaultString { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string FaultActor { get; set; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        string Details { get; set; }
    }
}