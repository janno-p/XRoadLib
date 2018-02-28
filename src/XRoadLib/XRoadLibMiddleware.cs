#if !NET452

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using XRoadLib.Handler;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public class XRoadLibMiddleware
    {
        private readonly RequestDelegate next;
        private readonly XRoadLibOptions options;
        private readonly Lazy<IXRoadHandler> wsdlHandler;
        private readonly Lazy<IXRoadHandler> requestHandler;

        public XRoadLibMiddleware(RequestDelegate next, IServiceProvider services, XRoadLibOptions options)
        {
            this.next = next;
            this.options = options;

            if (options.WsdlHandler != null)
                wsdlHandler = new Lazy<IXRoadHandler>(() => options.WsdlHandler(services));
            else if (options.ServiceManager != null)
                wsdlHandler = new Lazy<IXRoadHandler>(() => new XRoadWsdlHandler(options.ServiceManager(services)));
            else if (options.RequestHandler != null)
                wsdlHandler = new Lazy<IXRoadHandler>(() => new XRoadWsdlHandler(options.RequestHandler(services).ServiceManager));

            if (options.RequestHandler != null)
                requestHandler = new Lazy<IXRoadHandler>(() => options.RequestHandler(services));
            else if (options.ServiceManager != null)
                requestHandler = new Lazy<IXRoadHandler>(() => new XRoadRequestHandler(options.ServiceManager(services), options.StoragePath));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var handler = GetXRoadHandler(httpContext);
            if (handler == null)
            {
                await next.Invoke(httpContext);
                return;
            }

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

        private IXRoadHandler GetXRoadHandler(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value : null;

            if (httpContext.Request.Method == "GET" && Equals(path, options.WsdlPath))
                return wsdlHandler?.Value;

            if (httpContext.Request.Method == "POST" && Equals(path, options.RequestPath))
                return requestHandler?.Value;

            return null;
        }
    }
}

#endif
