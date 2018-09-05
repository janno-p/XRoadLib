using System;
using System.Collections.Generic;
using System.Linq;

namespace XRoadLib.Soap
{
    public class Soap12FaultException : Exception
    {
        public ISoap12Fault Fault { get; }

        public Soap12FaultException(ISoap12Fault fault)
            : base(fault?.Reason.First().Text)
        {
            Fault = fault ?? throw new ArgumentNullException(nameof(fault));
        }
    }
}