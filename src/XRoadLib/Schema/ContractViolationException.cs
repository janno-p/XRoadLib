using XRoadLib.Soap;

namespace XRoadLib.Schema
{
    public class ContractViolationException : XRoadException
    {
        public ContractViolationException(FaultCode faultCode, string message)
            : base(faultCode, message)
        { }
    }
}