using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore
{
    /// <summary>
    /// Base X-Road message handler for AspNetCore applications.
    /// </summary>
    public abstract class WebServiceHandler : IWebServiceHandler
    {
        public IServiceManager ServiceManager { get; }

        protected WebServiceHandler(IServiceManager serviceManager)
        {
            ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
        }

        /// <inheritdoc />
        public abstract Task HandleRequestAsync(WebServiceContext context);

        /// <inheritdoc />
        public virtual Task HandleExceptionAsync(WebServiceContext context, Exception exception, IFault fault)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(context.HttpContext.Response.Body, XRoadEncoding.Utf8)))
                context.MessageFormatter.WriteSoapFault(writer, fault);

            return Task.CompletedTask;
        }

        public virtual void Dispose()
        { }
    }
}