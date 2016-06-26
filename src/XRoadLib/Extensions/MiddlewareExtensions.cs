#if NETSTANDARD1_5

using Microsoft.AspNetCore.Builder;

namespace XRoadLib.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<XRoadLibMiddleware>();
        }
    }
}

#endif
