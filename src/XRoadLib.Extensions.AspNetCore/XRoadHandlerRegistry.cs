using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore
{
    public class XRoadHandlerRegistry
    {
        private readonly IDictionary<(string httpMethod, string route), Func<IServiceProvider, IXRoadHandler>> registeredHandlers = new Dictionary<(string httpMethod, string route), Func<IServiceProvider, IXRoadHandler>>();

        public Func<IServiceProvider, IXRoadHandler> FindHandler(HttpContext httpContext)
        {
            var httpMethod = httpContext.Request.Method;
            var route = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value : null;

            return registeredHandlers.TryGetValue((httpMethod, route), out var handler) ? handler : null;
        }
        
        public XRoadHandlerRegistry AddRequestHandler(string route, IServiceManager serviceManager)
        {
            registeredHandlers.Add(
                (HttpMethods.Post, route),
                serviceProvider => new XRoadRequestHandler(serviceProvider, serviceManager)
            );

            return this;
        }

        public XRoadHandlerRegistry AddRequestHandler<T>(string route)
            where T : IServiceManager
        {
            registeredHandlers.Add(
                (HttpMethods.Post, route),
                serviceProvider => new XRoadRequestHandler(serviceProvider, serviceProvider.GetRequiredService<T>())
            );

            return this;
        }
        
        public XRoadHandlerRegistry AddWsdlHandler(string route, IServiceManager serviceManager)
        {
            registeredHandlers.Add(
                (HttpMethods.Get, route),
                serviceProvider => new XRoadWsdlHandler(serviceManager)
            );

            return this;
        }

        public XRoadHandlerRegistry AddWsdlHandler<T>(string route)
            where T : IServiceManager
        {
            registeredHandlers.Add(
                (HttpMethods.Get, route),
                serviceProvider => new XRoadWsdlHandler(serviceProvider.GetRequiredService<T>())
            );

            return this;
        }

        internal void AddHandler(string httpMethod, string route, Func<IServiceProvider, IXRoadHandler> handlerFactory)
        {
            registeredHandlers.Add((httpMethod, route), handlerFactory);
        }
    }
}