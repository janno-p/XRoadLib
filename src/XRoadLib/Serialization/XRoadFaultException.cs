using System;

namespace XRoadLib.Serialization
{
    public class XRoadFaultException : Exception, IXRoadFault
    {
        public string FaultCode { get; }
        public string FaultString { get; }

        public XRoadFaultException(IXRoadFault xRoadFault)
        {
            FaultCode = xRoadFault.FaultCode;
            FaultString = xRoadFault.FaultString;
        }
    }
}