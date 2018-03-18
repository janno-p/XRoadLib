using System;
using XRoadLib.Soap;

namespace XRoadLib
{
    public abstract class XRoadException : Exception
    {
        public FaultCode FaultCode { get; }

        protected XRoadException(FaultCode faultCode, string message)
            : base(message)
        {
            FaultCode = faultCode;
        }
    }
}