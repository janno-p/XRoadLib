#if NETSTANDARD2_0

using System;
using System.IO;
using System.Xml;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    /// <summary>
    /// Base X-Road message handler for AspNetCore applications.
    /// </summary>
    public abstract class XRoadHandlerBase : IXRoadHandler
    {
        /// <summary>
        /// Handles X-Road message service request.
        /// </summary>
        public abstract void HandleRequest(HttpContext context);

        /// <summary>
        /// Handle exception that occured while handling X-Road message service request.
        /// </summary>
        public virtual void HandleException(HttpContext context, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(context.Response.Body, XRoadEncoding.UTF8)))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}

#endif
