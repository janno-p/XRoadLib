using System;
using System.IO;
using System.Text;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

#if !NETSTANDARD1_5
using System.Web;
#endif

namespace XRoadLib.Handler
{
    public abstract class ServiceHandlerBase : IHttpHandler
    {
        private static readonly Encoding encoding = XRoadEncoding.UTF8;

        protected XRoadMessage requestMessage;
        protected XRoadMessage responseMessage;

        public virtual bool IsReusable => false;

        public void ProcessRequest(HttpContext httpContext)
        {
            httpContext.Request.InputStream.Position = 0;
            httpContext.Response.ContentType = $"text/xml; charset={encoding.HeaderName}";

            using (requestMessage = new XRoadMessage())
            using (responseMessage = new XRoadMessage(new MemoryStream()))
            {
                try
                {
                    HandleRequest(httpContext);
                }
                catch (Exception exception)
                {
                    OnExceptionOccured(httpContext, exception, null, null, null, null);
                }
            }

            requestMessage = null;
            responseMessage = null;
        }

        protected abstract void HandleRequest(HttpContext httpContext);

        protected virtual void OnExceptionOccured(HttpContext httpContext, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = new XmlTextWriter(httpContext.Response.OutputStream, encoding))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}