using System;
using System.Threading.Tasks;
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
        Task HandleRequestAsync(WebServiceContext context);

        /// <summary>
        /// Handle exception that occured while handling X-Road message service request.
        /// </summary>
        Task HandleExceptionAsync(WebServiceContext context, Exception exception, IFault fault);
    }
}