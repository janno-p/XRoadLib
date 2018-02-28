using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore
{
    public class XRoadLibOptions
    {
        public Func<IServiceProvider, IServiceManager> ServiceManager { get; private set; }
        public Func<IServiceProvider, IXRoadHandler> WsdlHandler { get; private set; }
        public Func<IServiceProvider, IXRoadHandler> RequestHandler { get; private set; }

        public string WsdlPath { get; private set; } = "/";
        public string RequestPath { get; private set; } = "/";
        public DirectoryInfo StoragePath { get; private set; }

        public XRoadLibOptions WithServiceManager<T>(string path = "/") where T : IServiceManager
        {
            ServiceManager = serviceProvider => serviceProvider.GetRequiredService<T>();
            return WithRequestPath(path).WithWsdlPath(path);
        }

        public XRoadLibOptions WithServiceManager(IServiceManager serviceManager, string path = "/")
        {
            ServiceManager = serviceProvider => serviceManager;
            return WithRequestPath(path).WithWsdlPath(path);
        }

        public XRoadLibOptions WithServiceManager(Func<IServiceProvider, IServiceManager> serviceManager, string path = "/")
        {
            ServiceManager = serviceManager;
            return WithRequestPath(path).WithWsdlPath(path);
        }

        public XRoadLibOptions WithWsdlHandler<T>(string path = "/") where T : IXRoadHandler
        {
            WsdlHandler = serviceProvider => serviceProvider.GetRequiredService<T>();
            return WithWsdlPath(path);
        }

        public XRoadLibOptions WithWsdlHandler(IXRoadHandler wsdlHandler, string path = "/")
        {
            WsdlHandler = serviceProvider => wsdlHandler;
            return WithWsdlPath(path);
        }

        public XRoadLibOptions WithWsdlHandler(Func<IServiceProvider, IXRoadHandler> wsdlHandler, string path = "/")
        {
            WsdlHandler = wsdlHandler;
            return WithWsdlPath(path);
        }

        public XRoadLibOptions WithRequestHandler<T>(string path = "/") where T : IXRoadHandler
        {
            RequestHandler = serviceProvider => serviceProvider.GetRequiredService<T>();
            return WithRequestPath(path);
        }

        public XRoadLibOptions WithRequestHandler(IXRoadHandler requestHandler, string path = "/")
        {
            RequestHandler = serviceProvider => requestHandler;
            return WithRequestPath(path);
        }

        public XRoadLibOptions WithRequestHandler(Func<IServiceProvider, IXRoadHandler> requestHandler, string path = "/")
        {
            RequestHandler = requestHandler;
            return WithRequestPath(path);
        }

        public XRoadLibOptions WithRequestPath(string path)
        {
            RequestPath = string.IsNullOrWhiteSpace(path) ? "/" : path;
            return this;
        }

        public XRoadLibOptions WithStoragePath(DirectoryInfo path)
        {
            StoragePath = path;
            return this;
        }

        public XRoadLibOptions WithWsdlPath(string path)
        {
            WsdlPath = string.IsNullOrWhiteSpace(path) ? "/" : path;
            return this;
        }
    }
}