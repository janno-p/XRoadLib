using System;
using Microsoft.AspNetCore.Builder;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder builder)
        {
            return builder.UseXRoadLib(options => options);
        }

        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder builder, Func<XRoadLibOptions, XRoadLibOptions> optionBuilder)
        {
            return builder.UseMiddleware<XRoadLibMiddleware>(optionBuilder(new XRoadLibOptions()));
        }
    }
}