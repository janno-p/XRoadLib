#if !NET452

using System;
using System.Collections.Generic;
using System.Reflection;
using XRoadLib.Handler;

namespace XRoadLib
{
    public class XRoadLibOptions
    {
        public string WsdlPath { get; set; } = "/";
        public string RequestPath { get; set; } = "/";
        public Type WsdlHandler { get; private set; }
        public Type RequestHandler { get; private set; }
        public Assembly ContractAssembly { get; set; }
        public ICollection<IServiceManager> ServiceManagers { get; set; } = new List<IServiceManager>();
        public string StoragePath { get; set; }

        public void AddWsdlHandler<T>() where T : IXRoadHandler
        {
            WsdlHandler = typeof(T);
        }

        public void AddRequestHandler<T>() where T : IXRoadHandler
        {
            RequestHandler = typeof(T);
        }
    }
}

#endif
