using System;
using System.Diagnostics.CodeAnalysis;

namespace XRoadLib.Soap
{
    public class SoapFaultException : Exception
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")] 
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public ISoapFault Fault { get; }

        public SoapFaultException(ISoapFault fault)
            : base(fault?.FaultString)
        {
            Fault = fault ?? throw new ArgumentNullException(nameof(fault));
        }
    }
}