#if !NETSTANDARD2_0

using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    /// <inheritdoc />
    public class ServiceDescriptionHandlerBase : ServiceHandlerBase
    {
        /// <summary>
        /// X-Road protocol which description is provided by handler.
        /// </summary>
        protected virtual IServiceManager ServiceManager => null;

        /// <summary>
        /// DTO version of the service description.
        /// </summary>
        protected virtual uint? Version => null;

        /// <inheritdoc />
        protected override void HandleRequest(XRoadContextClassic context)
        {
            ServiceManager.CreateServiceDescription(version: Version)
                          .SaveTo(context.HttpContext.Response.OutputStream);
        }
    }
}

#endif