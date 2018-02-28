using System;
using System.IO;
using XRoadLib;
using XRoadLib.Extensions.AspNetCore;

namespace Calculator.Handler
{
    public class CalculatorHandler : XRoadRequestHandler
    {
        private readonly IServiceProvider serviceProvider;

        public CalculatorHandler(IServiceProvider serviceProvider, IServiceManager serviceManager, DirectoryInfo storagePath)
            : base(serviceManager, storagePath)
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