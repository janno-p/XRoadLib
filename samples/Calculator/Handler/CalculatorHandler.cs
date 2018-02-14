using System;
using System.Collections.Generic;
using XRoadLib;
using XRoadLib.Handler;
using XRoadLib.Serialization;

namespace Calculator.Handler
{
    public class CalculatorHandler : XRoadRequestHandler
    {
        private readonly IServiceProvider serviceProvider;

        public CalculatorHandler(IServiceProvider serviceProvider, IEnumerable<IServiceManager> serviceManagers, string storagePath)
            : base(serviceManagers, storagePath)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override object GetServiceObject(XRoadContext context)
        {
            var service = serviceProvider.GetService(context.ServiceMap.OperationDefinition.MethodInfo.DeclaringType);
            if (service != null)
                return service;

            throw new NotImplementedException();
        }
    }
}