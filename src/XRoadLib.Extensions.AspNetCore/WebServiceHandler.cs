using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNetCore
{
    /// <inheritdoc />
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
        public abstract void HandleRequest(WebServiceContext context);

        /// <inheritdoc />
        public virtual void HandleException(WebServiceContext context, Exception exception, IFault fault)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(context.HttpContext.Response.Body, XRoadEncoding.UTF8)))
                context.MessageFormatter.WriteSoapFault(writer, fault);
        }

        public virtual void Dispose()
        { }
    }
}