using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibRouteBuilderExtensions
    {
        public static IRouteBuilder MapWsdl(this IRouteBuilder builder, string template, IServiceManager serviceManager)
        {
            return builder.MapGet(template, async context =>
            {
                await ExecuteWsdlRequestDelegate(context, serviceManager);
            });
        }

        public static IRouteBuilder MapWsdl<T>(this IRouteBuilder builder, string template) where T : IServiceManager
        {
            return builder.MapGet(template, async context =>
            {
                await ExecuteWsdlRequestDelegate(context, context.RequestServices.GetRequiredService<T>());
            });
        }

        public static IRouteBuilder MapWsdlHandler<THandler>(this IRouteBuilder builder, string template)
            where THandler : IXRoadHandler
        {
            return builder.MapGet(template, async context =>
            {
                using (var handler = context.RequestServices.GetRequiredService<THandler>())
                    await XRoadLibMiddleware.Invoke(context, handler);
            });
        }

        public static IRouteBuilder MapWebService(this IRouteBuilder builder, string template, IServiceManager serviceManager)
        {
            return builder.MapPost(template, async context =>
            {
                await ExecuteWebServiceRequestDelegate(context, serviceManager);
            });
        }

        public static IRouteBuilder MapWebService<T>(this IRouteBuilder builder, string template) where T : IServiceManager
        {
            return builder.MapPost(template, async context =>
            {
                await ExecuteWebServiceRequestDelegate(context, context.RequestServices.GetRequiredService<T>());
            });
        }

        public static IRouteBuilder MapRequestHandler<THandler>(this IRouteBuilder builder, string template)
            where THandler : IXRoadHandler
        {
            return builder.MapPost(template, async context =>
            {
                using (var handler = context.RequestServices.GetRequiredService<THandler>())
                    await XRoadLibMiddleware.Invoke(context, handler);
            });
        }

        private static async Task ExecuteWsdlRequestDelegate(HttpContext context, IServiceManager serviceManager)
        {
            using (var handler = new XRoadWsdlHandler(serviceManager))
                await XRoadLibMiddleware.Invoke(context, handler);
        }

        private static async Task ExecuteWebServiceRequestDelegate(HttpContext context, IServiceManager serviceManager)
        {
            using (var handler = new XRoadRequestHandler(context.RequestServices, serviceManager))
                await XRoadLibMiddleware.Invoke(context, handler);
        }
    }
}