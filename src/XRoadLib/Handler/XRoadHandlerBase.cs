#if !NET452

using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    /// <inheritdoc />
    /// <summary>
    /// Base X-Road message handler for AspNetCore applications.
    /// </summary>
    public abstract class XRoadHandlerBase : IXRoadHandler
    {
        /// <inheritdoc />
        public abstract void HandleRequest(XRoadContext context);

        /// <inheritdoc />
        public virtual void HandleException(XRoadContext context, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(context.HttpContext.Response.Body, XRoadEncoding.UTF8)))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}

#endif
