#if !NET40

using System;
using Microsoft.AspNetCore.Http;
using XRoadLib.Soap;

namespace XRoadLib.Handler
{
    public interface IXRoadHandler
    {
        void HandleRequest(HttpContext context);
        void HandleException(HttpContext httpContext, Exception exception, FaultCode faultCode, string faultString, string faultActor, string details);
    }
}

#endif