using System;
using System.IO;
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
                    httpContext.Response.ContentType = XRoadHelper.GetContentTypeHeader(context.MessageFormatter.ContentType);

                    HandleRequest(context);
                }
                catch (Exception exception)
                {
                    if (context.MessageFormatter == null)
                        context.MessageFormatter = new SoapMessageFormatter();

                    httpContext.Response.ContentType = XRoadHelper.GetContentTypeHeader(context.MessageFormatter.ContentType);

                    if (httpContext.Response.OutputStream.CanSeek)
                    {
                        httpContext.Response.OutputStream.Seek(0, SeekOrigin.Begin);
                        httpContext.Response.OutputStream.SetLength(0);
                    }

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
            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, XRoadEncoding.Utf8))
                context.MessageFormatter.WriteSoapFault(writer, fault);
        }
    }
}