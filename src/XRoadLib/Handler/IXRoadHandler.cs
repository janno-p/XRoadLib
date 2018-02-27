#if !NET452

using System;
using Microsoft.AspNetCore.Http;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    /// <summary>
    /// General interface of X-Road message handlers.
    /// </summary>
    public interface IXRoadHandler
    {
        /// <summary>
        /// Handles X-Road message service request.
        /// </summary>
        void HandleRequest(HttpContext context);

        /// <summary>
        /// Handle exception that occured while handling X-Road message service request.
        /// </summary>
        void HandleException(HttpContext httpContext, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details);
    }
}

#endif
