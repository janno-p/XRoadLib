namespace XRoadLib.Extensions.AspNet
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
        protected override void HandleRequest(XRoadContext context)
        {
            ServiceManager.CreateServiceDescription(version: Version)
                          .SaveTo(context.HttpContext.Response.OutputStream);
        }
    }
}