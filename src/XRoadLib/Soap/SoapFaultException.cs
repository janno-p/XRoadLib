using System;

namespace XRoadLib.Soap
{
    public class SoapFaultException : Exception
    {
        public ISoapFault Fault { get; }

        public SoapFaultException(ISoapFault fault)
            : base(fault?.FaultString)
        {
            Fault = fault ?? throw new ArgumentNullException(nameof(fault));
        }
    }
}