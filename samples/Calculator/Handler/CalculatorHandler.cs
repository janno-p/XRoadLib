using System;
using System.Collections.Generic;
using XRoadLib.Handler;
using XRoadLib.Protocols;
using XRoadLib.Serialization.Mapping;

namespace Calculator.Handler
{
    public class CalculatorHandler : XRoadRequestHandler
    {
        private readonly IServiceProvider serviceProvider;

        public CalculatorHandler(IServiceProvider serviceProvider, IEnumerable<XRoadProtocol> supportedProtocols, string storagePath)
            : base(supportedProtocols, storagePath)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override object GetServiceObject(IServiceMap serviceMap)
        {
            var service = serviceProvider.GetService(serviceMap.Definition.MethodInfo.DeclaringType);
            if (service != null)
                return service;

            throw new NotImplementedException();
        }
    }
}