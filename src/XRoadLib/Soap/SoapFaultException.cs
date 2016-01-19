using System;

namespace XRoadLib.Soap
{
    public class SoapFaultException : Exception, ISoapFault
    {
        public string FaultCode { get; }
        public string FaultString { get; }
        public string FaultActor { get; }
        public string Details { get; }

        public SoapFaultException(ISoapFault soapFault)
            : base(soapFault?.FaultString)
        {
            if (soapFault == null)
                throw new ArgumentNullException(nameof(soapFault));

            FaultCode = soapFault.FaultCode;
            FaultString = soapFault.FaultString;
            FaultActor = soapFault.FaultActor;
            Details = soapFault.Details;
        }
    }
}