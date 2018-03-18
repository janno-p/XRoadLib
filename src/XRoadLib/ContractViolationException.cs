using XRoadLib.Soap;

namespace XRoadLib
{
    public class ContractViolationException : XRoadException
    {
        public ContractViolationException(FaultCode faultCode, string message)
            : base(faultCode, message)
        { }
    }
}