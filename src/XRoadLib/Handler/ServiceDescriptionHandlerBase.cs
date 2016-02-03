using System.Web;
using XRoadLib.Protocols;

namespace XRoadLib.Handler
{
    public class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        protected virtual Protocol Protocol => null;
        protected virtual uint? Version => null;

        protected override void HandleRequest(HttpContext httpContext)
        {
            Protocol.WriteServiceDescription(httpContext.Response.OutputStream, Version);
        }
    }
}