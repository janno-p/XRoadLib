using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions.AspNetCore
{
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
        public override async Task HandleRequestAsync(WebServiceContext context)
        {
            var writer = XmlWriter.Create(context.HttpContext.Response.Body, new XmlWriterSettings
            {
                Async = true,
                Encoding = XRoadEncoding.Utf8,
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n"
            });

            await ServiceManager.WriteServiceDefinitionAsync(writer).ConfigureAwait(false);
        }
    }
}