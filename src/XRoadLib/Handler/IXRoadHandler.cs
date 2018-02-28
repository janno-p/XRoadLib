#if !NET452

using System;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    /// <summary>
    /// General interface of X-Road message handlers.
    /// </summary>
    public interface IXRoadHandler
    {
        /// <summary>
        /// Provides services which this handler instance supports.
        /// </summary>
        IServiceManager ServiceManager { get; }

        /// <summary>
        /// Handles X-Road message service request.
        /// </summary>
        void HandleRequest(XRoadContext context);

        /// <summary>
        /// Handle exception that occured while handling X-Road message service request.
        /// </summary>
        void HandleException(XRoadContext httpContext, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details);
    }
}

#endif
