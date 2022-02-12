﻿using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore;

public static class XRoadLibRouteBuilderExtensions
{
    [UsedImplicitly]
    public static IRouteBuilder MapWsdl(this IRouteBuilder builder, string template, IServiceManager serviceManager)
    {
        return builder.MapGet(template, context => ExecuteWsdlRequestDelegate(context, serviceManager));
    }

    [UsedImplicitly]
    public static IRouteBuilder MapWsdl<T>(this IRouteBuilder builder, string template) where T : IServiceManager
    {
        return builder.MapGet(template, context => ExecuteWsdlRequestDelegate(context, context.RequestServices.GetRequiredService<T>()));
    }

    [UsedImplicitly]
    public static IRouteBuilder MapWsdlHandler<THandler>(this IRouteBuilder builder, string template)
        where THandler : IWebServiceHandler
    {
        return builder.MapGet(template, async context =>
        {
            var handler = context.RequestServices.GetRequiredService<THandler>();
            await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
        });
    }

    [UsedImplicitly]
    public static IRouteBuilder MapWebService(this IRouteBuilder builder, string template, IServiceManager serviceManager)
    {
        return builder.MapPost(template, context => ExecuteWebServiceRequestDelegate(context, serviceManager));
    }

    [UsedImplicitly]
    public static IRouteBuilder MapWebService<T>(this IRouteBuilder builder, string template) where T : IServiceManager
    {
        return builder.MapPost(template, context => ExecuteWebServiceRequestDelegate(context, context.RequestServices.GetRequiredService<T>()));
    }

    [UsedImplicitly]
    public static IRouteBuilder MapRequestHandler<THandler>(this IRouteBuilder builder, string template)
        where THandler : IWebServiceHandler
    {
        return builder.MapPost(template, async context =>
        {
            var handler = context.RequestServices.GetRequiredService<THandler>();
            await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
        });
    }

    public static async Task ExecuteWsdlRequest<T>(this HttpContext context) where T : IServiceManager
    {
        var handler = new WebServiceDescriptionHandler(context.RequestServices.GetRequiredService<T>());
        await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
    }

    public static async Task ExecuteWebServiceRequest<T>(this HttpContext context) where T : IServiceManager
    {
        var handler = new WebServiceRequestHandler(context.RequestServices, context.RequestServices.GetRequiredService<T>());
        await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
    }

    private static async Task ExecuteWsdlRequestDelegate(HttpContext context, IServiceManager serviceManager)
    {
        var handler = new WebServiceDescriptionHandler(serviceManager);
        await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
    }

    private static async Task ExecuteWebServiceRequestDelegate(HttpContext context, IServiceManager serviceManager)
    {
        var handler = new WebServiceRequestHandler(context.RequestServices, serviceManager);
        await XRoadLibMiddleware.Invoke(context, handler).ConfigureAwait(false);
    }
}