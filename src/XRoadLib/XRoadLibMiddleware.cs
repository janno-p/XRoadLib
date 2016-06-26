#if NETSTANDARD1_5

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.OptionsModel;

namespace XRoadLib
{
    public class XRoadLibMiddleware
    {
        private readonly RequestDelegate next;
        private readonly XRoadLibOptions options;

        public XRoadLibMiddleware(RequestDelegate next, IOptions<XRoadLibOptions> options)
        {
            this.next = next;
            this.options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            await next.Invoke(context);
        }
    }
}

#endif
