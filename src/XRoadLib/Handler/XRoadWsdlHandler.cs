#if NETSTANDARD1_5

using System;
using Microsoft.AspNetCore.Http;
using XRoadLib.Protocols;

namespace XRoadLib.Handler
{
    public class XRoadWsdlHandler : XRoadHandlerBase
    {
        private readonly XRoadProtocol protocol;

        public XRoadWsdlHandler(XRoadProtocol protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));
            this.protocol = protocol;
        }

        public override void HandleRequest(HttpContext context)
        {
            protocol.WriteServiceDescription(context.Response.Body);
        }
    }
}

#endif