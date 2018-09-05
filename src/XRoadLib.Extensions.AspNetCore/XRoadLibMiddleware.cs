using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibMiddleware
    {
        public static Task Invoke(HttpContext httpContext, IWebServiceHandler handler)
        {
            var options = httpContext.RequestServices.GetRequiredService<XRoadLibOptions>();

            if (handler is WebServiceRequestHandler requestHandler)
                requestHandler.StoragePath = options.StoragePath;

            using (var context = new WebServiceContext(httpContext))
            {
                try
                {
                    context.MessageFormatter = XRoadHelper.GetMessageFormatter(context.HttpContext.Request.ContentType);

                    if (httpContext.Request.Body.CanSeek)
                        httpContext.Request.Body.Position = 0;

                    httpContext.Response.ContentType = $"{context.MessageFormatter.ContentType}; charset={XRoadEncoding.UTF8.WebName}";

                    handler.HandleRequest(context);
                }
                catch (Exception exception)
                {
                    httpContext.Response.ContentType = context.MessageFormatter.ContentType;

                    if (httpContext.Response.Body.CanSeek)
                    {
                        httpContext.Response.Body.Position = 0;
                        httpContext.Response.Body.SetLength(0);
                    }

                    var fault = context.MessageFormatter.CreateFault(exception);

                    handler.HandleException(context, exception, fault);
                }
            }

            return Task.FromResult(0);
        }
    }
}