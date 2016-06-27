#if NETSTANDARD1_5

using System.IO;
using Microsoft.AspNetCore.Http;
using XRoadLib.Serialization;

namespace XRoadLib.Handler
{
    public class DefaultRequestHandler : XRoadHandlerBase
    {
        public override void HandleRequest(HttpContext context)
        {
            using (var requestMessage = new XRoadMessage())
            using (var responseMessage = new XRoadMessage(new MemoryStream()))
            {

            }
        }
    }
}

#endif