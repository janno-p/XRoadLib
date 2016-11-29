using System;
using Microsoft.AspNetCore.Builder;

namespace XRoadLib.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder builder)
        {
            return builder.UseXRoadLib(options => {});
        }

        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder builder, Action<XRoadLibOptions> optionBuilder)
        {
            var options = new XRoadLibOptions();
            optionBuilder(options);
            return builder.UseMiddleware<XRoadLibMiddleware>(options);
        }
    }
}
