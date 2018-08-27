using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibMiddleware
    {
        public static Task Invoke(HttpContext httpContext, IXRoadHandler handler)
        {
            var options = httpContext.RequestServices.GetRequiredService<XRoadLibOptions>();

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

            return Task.FromResult(0);
        }
    }
}