using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore;

/// <summary>
/// X-Road context of classical AspNetCore applications.
/// </summary>
[UsedImplicitly]
public sealed class WebServiceContext : IDisposable
{
    /// <summary>
    /// HTTP context this X-Road context is bound to.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// X-Road request message.
    /// </summary>
    public XRoadMessage Request { get; }

    /// <summary>
    /// X-Road response message.
    /// </summary>
    public XRoadMessage Response { get; }

    /// <summary>
    /// ServiceMap which maps to operation of the message.
    /// </summary>
    public IServiceMap? ServiceMap { get; set; }

    /// <summary>
    /// Deserialized request parameters.
    /// </summary>
    public object? Parameters { get; set; }

    /// <summary>
    /// Result of the X-Road service request.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Exception that occurred while handling service request.
    /// </summary>
    [UsedImplicitly]
    public Exception? Exception { get; set; }

    public IMessageFormatter? MessageFormatter { get; set; }

    /// <summary>
    /// Initialize new X-Road context instance.
    /// </summary>
    public WebServiceContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        Request = new XRoadMessage();
        Response = new XRoadMessage(new MemoryStream());
    }

    void IDisposable.Dispose()
    {
        Response.Dispose();
        Request.Dispose();
    }
}