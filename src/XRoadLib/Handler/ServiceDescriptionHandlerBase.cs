#if !NETSTANDARD1_5

using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    /// <summary>
    /// Handler of X-Road service description.
    /// </summary>
    public class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        /// <summary>
        /// X-Road protocol which description is provided by handler.
        /// </summary>
        protected virtual XRoadProtocol Protocol => null;

        /// <summary>
        /// DTO version of the service description.
        /// </summary>
        protected virtual uint? Version => null;

        /// <summary>
        /// Handles service description request.
        /// </summary>
        protected override void HandleRequest(XRoadContextClassic context)
        {
            Protocol.WriteServiceDescription(context.HttpContext.Response.OutputStream, Version);
        }
    }
}

#endif