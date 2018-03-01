using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddXRoadLib(this IServiceCollection serviceCollection, Action<XRoadHandlerRegistry> initializer = null)
        {
            return serviceCollection.AddXRoadLib(null, initializer);
        }

        public static IServiceCollection AddXRoadLib<T>(this IServiceCollection serviceCollection, Action<XRoadHandlerRegistry> initializer = null)
        {
            return serviceCollection.AddXRoadLib(typeof(T).GetTypeInfo().Assembly, initializer);
        }

        public static IServiceCollection AddXRoadLib(this IServiceCollection serviceCollection, Assembly assembly, Action<XRoadHandlerRegistry> initializer = null)
        {
            var registry = new XRoadHandlerRegistry();

            initializer?.Invoke(registry);

            if (assembly != null)
                RegisterAssemblyDefinedHandlers(serviceCollection, registry, assembly);

            return serviceCollection.AddSingleton(registry);
        }

        private static void RegisterAssemblyDefinedHandlers(IServiceCollection serviceCollection, XRoadHandlerRegistry registry, Assembly assembly)
        {
            foreach (var handlerType in assembly.GetExportedTypes().Where(t => Attribute.IsDefined(t, typeof(XRoadHandlerAttribute))))
            {
                var attribute = handlerType.GetCustomAttribute<XRoadHandlerAttribute>();
                serviceCollection.AddTransient(handlerType);
                registry.AddHandler(attribute.HttpMethod, attribute.Route, serviceProvider => (IXRoadHandler)serviceProvider.GetRequiredService(handlerType));
            }
        }
    }
}