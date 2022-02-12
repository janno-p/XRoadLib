using JetBrains.Annotations;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore;

/// <summary>
/// General interface of X-Road message handlers.
/// </summary>
public interface IWebServiceHandler
{
    /// <summary>
    /// Provides services which this handler instance supports.
    /// </summary>
    [UsedImplicitly]
    IServiceManager ServiceManager { get; }

    /// <summary>
    /// Handles X-Road message service request.
    /// </summary>
    Task HandleRequestAsync(WebServiceContext context);

    /// <summary>
    /// Handle exception that occurred while handling X-Road message service request.
    /// </summary>
    [UsedImplicitly]
    Task HandleExceptionAsync(WebServiceContext context, Exception exception, IFault fault);
}