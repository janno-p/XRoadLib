#if NETSTANDARD1_5

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

            wsdlHandler = new Lazy<IXRoadHandler>(() => options.WsdlHandler != null ? (IXRoadHandler)services.GetRequiredService(options.WsdlHandler) : new XRoadWsdlHandler(options.SupportedProtocols.FirstOrDefault()));
            requestHandler = new Lazy<IXRoadHandler>(() => options.RequestHandler != null ? (IXRoadHandler)services.GetRequiredService(options.RequestHandler) : new XRoadRequestHandler(options.SupportedProtocols, options.StoragePath));
        }

        public async Task Invoke(HttpContext context)
        {
            var handler = GetXRoadHandler(context);
            if (handler != null)
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
            if (context.Request.Method == "GET" && context.Request.Path.Equals(options.WsdlPath))
                return wsdlHandler.Value;

            if (context.Request.Method == "POST" && context.Request.Path.Equals(options.RequestPath))
                return requestHandler.Value;

            return null;
        }
    }
}

#endif