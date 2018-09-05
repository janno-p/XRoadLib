using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadMessageExtensions
    {
        /// <summary>
        /// Loads X-Road message contents from request message.
        /// </summary>
        public static void LoadRequest(this XRoadMessage message, HttpContext httpContext, IMessageFormatter messageFormatter, string storagePath, IEnumerable<IServiceManager> serviceManagers)
        {
            message.LoadRequest(httpContext.Request.Body, messageFormatter, httpContext.Request.Headers.GetContentTypeHeader(), storagePath, serviceManagers);
        }

        /// <summary>
        /// Loads X-Road message contents from request message.
        /// </summary>
        public static void LoadRequest(this XRoadMessage message, HttpContext httpContext, IMessageFormatter messageFormatter, string storagePath, IServiceManager serviceManager)
        {
            message.LoadRequest(httpContext.Request.Body, messageFormatter, httpContext.Request.Headers.GetContentTypeHeader(), storagePath, serviceManager);
        }

        /// <summary>
        /// Serializes X-Road message into specified HTTP context response.
        /// </summary>
        public static void SaveTo(this XRoadMessage message, HttpContext httpContext, IMessageFormatter messageFormatter)
        {
            var outputStream = httpContext.Response.Body;
            var appendHeader = new Action<string, string>((k, v) => httpContext.Response.Headers[k] = v);

            using (var writer = new XRoadMessageWriter(outputStream))
                writer.Write(message, contentType => httpContext.Response.ContentType = contentType, appendHeader, messageFormatter);
        }
    }
}