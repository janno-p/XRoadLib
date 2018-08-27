using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibMiddlewareExtensions
    {
        public static IApplicationBuilder UseXRoadLib(this IApplicationBuilder app, Action<IRouteBuilder> configureRoutes)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            if (configureRoutes == null)
                throw new ArgumentNullException(nameof(configureRoutes));

            var routes = new RouteBuilder(app);

            configureRoutes(routes);

            return app.UseRouter(routes.Build());
        }
    }
}