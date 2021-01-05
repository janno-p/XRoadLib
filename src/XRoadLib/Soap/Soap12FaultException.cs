using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace XRoadLib.Soap
{
    public class Soap12FaultException : Exception
    {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public ISoap12Fault Fault { get; }

        public Soap12FaultException(ISoap12Fault fault)
            : base(fault?.Reason.First().Text)
        {
            Fault = fault ?? throw new ArgumentNullException(nameof(fault));
        }
    }
}