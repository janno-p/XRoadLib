#if !NETSTANDARD1_5

using System;
using System.Text;
using System.Xml;
using System.Web;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Handler
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

            using (var context = new XRoadContextClassic(httpContext))
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
        protected abstract void HandleRequest(XRoadContextClassic context);

        /// <summary>
        /// Handles all exceptions as technical SOAP faults.
        /// </summary>
        protected virtual void OnExceptionOccured(XRoadContextClassic context, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = new XmlTextWriter(context.HttpContext.Response.OutputStream, encoding))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}

#endif