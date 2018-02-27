#if !NET452

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

            if (!options.ServiceManagers.Any())
                throw new ArgumentException("At least one supported protocol definition is required.", nameof(options));

            wsdlHandler = new Lazy<IXRoadHandler>(() => options.WsdlHandler != null ? (IXRoadHandler)services.GetRequiredService(options.WsdlHandler) : new XRoadWsdlHandler(options.ServiceManagers.FirstOrDefault()));
            requestHandler = new Lazy<IXRoadHandler>(() => options.RequestHandler != null ? (IXRoadHandler)services.GetRequiredService(options.RequestHandler) : new XRoadRequestHandler(options.ServiceManagers, options.StoragePath));
        }

        public async Task Invoke(HttpContext context)
        {
            var handler = GetXRoadHandler(context);
            if (handler == null)
            {
                await next.Invoke(context);
                return;
            }

            try
            {
                if (context.Request.Body.CanSeek)
                    context.Request.Body.Position = 0;

                context.Response.ContentType = $"text/xml; charset={XRoadEncoding.UTF8.WebName}";

                handler.HandleRequest(context);
            }
            catch (Exception exception)
            {
                if (context.Response.Body.CanSeek)
                {
                    context.Response.Body.Position = 0;
                    context.Response.Body.SetLength(0);
                }

                handler.HandleException(context, exception, null, null, null, null);
            }
        }

        private IXRoadHandler GetXRoadHandler(HttpContext context)
        {
            var path = context.Request.Path.HasValue ? context.Request.Path.Value : null;

            if (context.Request.Method == "GET" && Equals(path, options.WsdlPath))
                return wsdlHandler.Value;

            if (context.Request.Method == "POST" && Equals(path, options.RequestPath))
                return requestHandler.Value;

            return null;
        }
    }
}

#endif
