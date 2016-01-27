using System.Reflection;
using System.Web;
using XRoadLib.Protocols;

namespace XRoadLib.Handler
{
    public class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        private readonly Assembly contractAssembly;

        protected virtual IProtocol Protocol => null;

        protected ServiceDescriptionHandlerBase(Assembly contractAssembly)
        {
            this.contractAssembly = contractAssembly;
        }

        protected override void HandleRequest(HttpContext context)
        {
            Protocol.WriteServiceDescription(contractAssembly, context.Response.OutputStream);
        }
    }
}