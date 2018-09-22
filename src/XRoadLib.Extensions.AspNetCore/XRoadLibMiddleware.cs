using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibMiddleware
    {
        public static async Task Invoke(HttpContext httpContext, IWebServiceHandler handler)
        {
            var options = httpContext.RequestServices.GetRequiredService<XRoadLibOptions>();
            var accessor = httpContext.RequestServices.GetRequiredService<IWebServiceContextAccessor>();

            if (handler is WebServiceRequestHandler requestHandler)
                requestHandler.StoragePath = options.StoragePath;

            using (var context = new WebServiceContext(httpContext))
            {
                if (accessor is WebServiceContextAccessor webServiceContextAccessor)
                    webServiceContextAccessor.WebServiceContext = context;
                
                try
                {
                    context.MessageFormatter = XRoadHelper.GetMessageFormatter(context.HttpContext.Request.ContentType);

                    if (httpContext.Request.Body.CanSeek)
                        httpContext.Request.Body.Position = 0;

                    httpContext.Response.ContentType = $"{context.MessageFormatter.ContentType}; charset={XRoadEncoding.UTF8.WebName}";

                    await handler.HandleRequestAsync(context);
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

                    await handler.HandleExceptionAsync(context, exception, fault);
                }
            }
        }
    }
}