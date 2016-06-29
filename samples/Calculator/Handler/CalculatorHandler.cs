using System;
using System.Collections.Generic;
using Calculator.Contract;
using Calculator.WebService;
using XRoadLib.Handler;
using XRoadLib.Protocols;
using XRoadLib.Serialization.Mapping;

namespace Calculator.Handler
{
    public class CalculatorHandler : XRoadRequestHandler
    {
        public CalculatorHandler(IEnumerable<XRoadProtocol> supportedProtocols, string storagePath)
            : base(supportedProtocols, storagePath)
        { }

        protected override object GetServiceObject(IServiceMap serviceMap)
        {
            if (serviceMap.Definition.MethodInfo.DeclaringType == typeof(ISumOfIntegers))
                return new SumOfIntegersWebService();

            throw new NotImplementedException();
        }
    }
}