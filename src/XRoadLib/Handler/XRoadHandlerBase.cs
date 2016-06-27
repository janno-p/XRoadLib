#if NETSTANDARD1_5

using System;
using System.IO;
using System.Xml;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    public abstract class XRoadHandlerBase : IXRoadHandler
    {
        public virtual void HandleRequest(HttpContext context)
        {

        }

        public virtual void HandleException(HttpContext context, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details)
        {
            using (var writer = XmlWriter.Create(new StreamWriter(context.Response.Body, XRoadEncoding.UTF8)))
                SoapMessageHelper.SerializeSoapFaultResponse(writer, faultCode, faultString, faultActor, details, exception);
        }
    }
}

#endif