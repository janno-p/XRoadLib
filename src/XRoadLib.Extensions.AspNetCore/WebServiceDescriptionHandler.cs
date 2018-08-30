namespace XRoadLib.Extensions.AspNetCore
{
    /// <inheritdoc />
    /// <summary>
    /// Handle X-Road service description request on AspNetCore platform.
    /// </summary>
    public class WebServiceDescriptionHandler : WebServiceHandler
    {
        /// <summary>
        /// Initialize new handler for certain protocol.
        /// </summary>
        public WebServiceDescriptionHandler(IServiceManager serviceManager)
            : base(serviceManager)
        { }

        /// <inheritdoc />
        public override void HandleRequest(WebServiceContext context)
        {
            ServiceManager.CreateServiceDescription()
                          .SaveTo(context.HttpContext.Response.Body);
        }
    }
}