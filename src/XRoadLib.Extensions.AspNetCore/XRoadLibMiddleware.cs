using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    public class XRoadLibMiddleware
    {
        private readonly IServiceProvider serviceProvider;
        private readonly RequestDelegate next;
        private readonly XRoadLibOptions options;
        private readonly XRoadHandlerRegistry registry;

        public XRoadLibMiddleware(RequestDelegate next, IServiceProvider serviceProvider, XRoadHandlerRegistry registry, XRoadLibOptions options)
        {
            this.next = next;
            this.options = options;
            this.registry = registry;
            this.serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var handlerFactory = registry.FindHandler(httpContext);
            if (handlerFactory == null)
            {
                await next.Invoke(httpContext);
                return;
            }

            var handler = handlerFactory(serviceProvider);
            if (handler is XRoadRequestHandler requestHandler)
                requestHandler.StoragePath = options.StoragePath;

            using (var context = new XRoadContext(httpContext))
            {
                try
                {
                    if (httpContext.Request.Body.CanSeek)
                        httpContext.Request.Body.Position = 0;

                    httpContext.Response.ContentType = $"text/xml; charset={XRoadEncoding.UTF8.WebName}";

                    handler.HandleRequest(context);
                }
                catch (Exception exception)
                {
                    if (httpContext.Response.Body.CanSeek)
                    {
                        httpContext.Response.Body.Position = 0;
                        httpContext.Response.Body.SetLength(0);
                    }

                    handler.HandleException(context, exception, null, null, null, null);
                }
            }
        }
    }
}