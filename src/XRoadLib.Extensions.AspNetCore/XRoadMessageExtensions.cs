using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadMessageExtensions
    {
        /// <summary>
        /// Loads X-Road message contents from request message.
        /// </summary>
        public static void LoadRequest(this XRoadMessage message, HttpContext httpContext, string storagePath, IEnumerable<IServiceManager> serviceManagers)
        {
            message.LoadRequest(httpContext.Request.Body, httpContext.Request.Headers.GetContentTypeHeader(), storagePath, serviceManagers);
        }

        /// <summary>
        /// Loads X-Road message contents from request message.
        /// </summary>
        public static void LoadRequest(this XRoadMessage message, HttpContext httpContext, string storagePath, IServiceManager serviceManager)
        {
            message.LoadRequest(httpContext.Request.Body, httpContext.Request.Headers.GetContentTypeHeader(), storagePath, new[] { serviceManager });
        }

        /// <summary>
        /// Serializes X-Road message into specified HTTP context response.
        /// </summary>
        public static void SaveTo(this XRoadMessage message, HttpContext httpContext)
        {
            var outputStream = httpContext.Response.Body;
            var appendHeader = new Action<string, string>((k, v) => httpContext.Response.Headers[k] = v);

            using (var writer = new XRoadMessageWriter(outputStream))
                writer.Write(message, contentType => httpContext.Response.ContentType = contentType, appendHeader);
        }
    }
}