using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore;

/// <summary>
/// Base X-Road message handler for AspNetCore applications.
/// </summary>
public abstract class WebServiceHandler : IWebServiceHandler
{
    protected readonly IMessageFormatter DefaultMessageFormatter = new SoapMessageFormatter();

    public IServiceManager ServiceManager { get; }

    protected WebServiceHandler(IServiceManager serviceManager)
    {
        ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
    }

    /// <inheritdoc />
    public abstract Task HandleRequestAsync(WebServiceContext context);

    /// <inheritdoc />
    public virtual async Task HandleExceptionAsync(WebServiceContext context, Exception exception, IFault fault)
    {
        var writer = XmlWriter.Create(context.HttpContext.Response.Body, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 });

        await (context.MessageFormatter ?? DefaultMessageFormatter).WriteSoapFaultAsync(writer, fault).ConfigureAwait(false);

        await writer.FlushAsync().ConfigureAwait(false);
    }

    public virtual void Dispose()
    { }
}