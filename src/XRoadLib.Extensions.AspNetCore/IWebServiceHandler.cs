using System.Diagnostics.CodeAnalysis;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore;

/// <summary>
/// General interface of X-Road message handlers.
/// </summary>
public interface IWebServiceHandler : IDisposable
{
    /// <summary>
    /// Provides services which this handler instance supports.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    IServiceManager ServiceManager { get; }

    /// <summary>
    /// Handles X-Road message service request.
    /// </summary>
    Task HandleRequestAsync(WebServiceContext context);

    /// <summary>
    /// Handle exception that occurred while handling X-Road message service request.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    Task HandleExceptionAsync(WebServiceContext context, Exception exception, IFault fault);
}