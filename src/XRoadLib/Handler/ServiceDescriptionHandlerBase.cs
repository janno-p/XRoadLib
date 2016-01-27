using System.Reflection;
using System.Web;
using XRoadLib.Description;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;

namespace XRoadLib.Handler
{
    public abstract class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        protected readonly Assembly contractAssembly;

        protected virtual IProtocol Protocol => null;
        protected virtual uint? Version => null;

        protected ServiceDescriptionHandlerBase(Assembly contractAssembly)
        {
            this.contractAssembly = contractAssembly;
        }

        protected override void HandleRequest(HttpContext context)
        {
            var definition = new ProducerDefinition(contractAssembly, Protocol, Version, EnvironmentProducerName);

            OnPrepareHeaders(definition);
            OnPrepareDefinition(definition);

            definition.SaveTo(context.Response.OutputStream);
        }

        protected virtual void OnPrepareDefinition(ProducerDefinition definition)
        { }
    }
}