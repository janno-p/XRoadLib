using System;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore
{
    /// <summary>
    /// General interface of X-Road message handlers.
    /// </summary>
    public interface IWebServiceHandler : IDisposable
    {
        /// <summary>
        /// Provides services which this handler instance supports.
        /// </summary>
        IServiceManager ServiceManager { get; }

        /// <summary>
        /// Handles X-Road message service request.
        /// </summary>
        void HandleRequest(WebServiceContext context);

        /// <summary>
        /// Handle exception that occured while handling X-Road message service request.
        /// </summary>
        void HandleException(WebServiceContext context, Exception exception, IFault fault);
    }
}