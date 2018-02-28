#if !NET452

using System;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    /// <inheritdoc />
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
            this.serviceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        }

        /// <inheritdoc />
        public override void HandleRequest(XRoadContext context)
        {
            serviceManager.CreateServiceDescription()
                          .SaveTo(context.HttpContext.Response.Body);
        }
    }
}

#endif
