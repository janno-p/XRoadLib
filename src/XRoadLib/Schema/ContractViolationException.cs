using System.Diagnostics.CodeAnalysis;
using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class ContractViolationException : XRoadException
    {
        [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
        public ContractViolationException(FaultCode faultCode, string message)
            : base(faultCode, message)
        { }
    }
}