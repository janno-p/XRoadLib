#if NETSTANDARD2_0

using System;
using Microsoft.AspNetCore.Http;
using XRoadLib.Extensions;

namespace XRoadLib.Handler
{
    /// <summary>
    /// Handle X-Road service description request on AspNetCore platform.
    /// </summary>
    public class XRoadWsdlHandler : XRoadHandlerBase
    {
        private readonly IServiceManager serviceManager;

        /// <summary>
        /// Initialize new handler for certain protocol.
        /// </summary>
        public XRoadWsdlHandler(IServiceManager serviceManager)
        {
            if (serviceManager == null)
                throw new ArgumentNullException(nameof(serviceManager));
            this.serviceManager = serviceManager;
        }

        /// <summary>
        /// Handle service description request.
        /// </summary>
        public override void HandleRequest(HttpContext context)
        {
            serviceManager.CreateServiceDescription()
                          .SaveTo(context.Response.Body);
        }
    }
}

#endif
