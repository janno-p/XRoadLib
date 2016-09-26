#if !NETSTANDARD1_5

using System.Web;

namespace XRoadLib.Handler
{
    public class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        protected virtual XRoadProtocol Protocol => null;
        protected virtual uint? Version => null;

        protected override void HandleRequest(HttpContext httpContext)
        {
            Protocol.WriteServiceDescription(httpContext.Response.OutputStream, Version);
        }
    }
}

#endif