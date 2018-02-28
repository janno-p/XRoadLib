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
            httpContext.Request.InputStream.Position = 0;
            httpContext.Response.ContentType = $"text/xml; charset={encoding.HeaderName}";

            using (var context = new XRoadContext(httpContext))
            {
                try
                {
                    HandleRequest(context);
                }
                catch (Exception exception)
                {
                    OnExceptionOccured(context, exception, null, null, null, null);
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
        protected virtual void OnExceptionOccured(XRoadContext context, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, encoding))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}