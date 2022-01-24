﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore;

public static class XRoadLibMiddleware
{
    public static async Task Invoke(HttpContext httpContext, IWebServiceHandler handler)
    {
        var options = httpContext.RequestServices.GetRequiredService<XRoadLibOptions>();
        var accessor = httpContext.RequestServices.GetRequiredService<IWebServiceContextAccessor>();
        var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(XRoadLibMiddleware));

        if (handler is WebServiceRequestHandler requestHandler)
            requestHandler.StoragePath = options.StoragePath;

        using var context = new WebServiceContext(httpContext);
        if (accessor is WebServiceContextAccessor webServiceContextAccessor)
            webServiceContextAccessor.WebServiceContext = context;
                
        try
        {
            context.MessageFormatter = XRoadHelper.GetMessageFormatter(context.HttpContext.Request.ContentType);

            if (httpContext.Request.Body.CanSeek)
                httpContext.Request.Body.Position = 0;

            httpContext.Response.ContentType = XRoadHelper.GetContentTypeHeader(context.MessageFormatter.ContentType);

            await handler.HandleRequestAsync(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error while processing X-Road request");

            if (context.MessageFormatter == null)
            {
                context.MessageFormatter = new SoapMessageFormatter();
                httpContext.Response.ContentType = XRoadHelper.GetContentTypeHeader(context.MessageFormatter.ContentType);
            }

            if (httpContext.Response.Body.CanSeek)
            {
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                httpContext.Response.Body.SetLength(0);
            }

            var fault = context.MessageFormatter.CreateFault(exception);

            await handler.HandleExceptionAsync(context, exception, fault).ConfigureAwait(false);
        }
    }
}