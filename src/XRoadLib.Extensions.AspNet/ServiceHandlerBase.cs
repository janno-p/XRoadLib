using System;
using System.Text;
using System.Web;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Extensions.AspNet
{
    /// <summary>
    /// Base handler of various X-Road operations.
    /// </summary>
    public abstract class ServiceHandlerBase : IHttpHandler
    {
        private static readonly Encoding encoding = XRoadEncoding.UTF8;

        /// <summary>
        /// Overrides IHttpHandler.IsReusable.
        /// </summary>
        public virtual bool IsReusable => false;

        /// <summary>
        /// Handle incoming request.
        /// </summary>
        public virtual void ProcessRequest(HttpContext httpContext)
        {
            using (var context = new XRoadContext(httpContext))
            {
                try
                {
                    context.MessageFormatter = XRoadHelper.GetMessageFormatter(httpContext.Request.ContentType);

                    httpContext.Request.InputStream.Position = 0;

                    httpContext.Response.ContentType = $"{context.MessageFormatter.ContentType}; charset={encoding.HeaderName}";

                    HandleRequest(context);
                }
                catch (Exception exception)
                {
                    var fault = context.MessageFormatter.CreateFault(exception);

                    OnExceptionOccurred(context, exception, fault);
                }
            }
        }

        /// <summary>
        /// Handle current X-Road operation.
        /// </summary>
        protected abstract void HandleRequest(XRoadContext context);

        /// <summary>
        /// Handles all exceptions as technical SOAP faults.
        /// </summary>
        protected virtual void OnExceptionOccurred(XRoadContext context, Exception exception, IFault fault)
        {
            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, encoding))
                context.MessageFormatter.WriteSoapFault(writer, fault);
        }
    }
}