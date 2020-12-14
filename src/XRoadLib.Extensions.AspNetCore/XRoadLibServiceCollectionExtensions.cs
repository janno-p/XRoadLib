using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore
{
    public static class XRoadLibServiceCollectionExtensions
    {
        public static IServiceCollection AddXRoadLib(this IServiceCollection services, Func<IServiceProvider, Func<object, Task<object>>> handlerFactory) =>
            AddXRoadLib(services, _ => {}, handlerFactory);

        public static IServiceCollection AddXRoadLib(this IServiceCollection services, Action<XRoadLibOptions> configureOptions, Func<IServiceProvider, Func<object, Task<object>>> handlerFactory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            services.AddRouting();
            services.AddScoped<IWebServiceContextAccessor, WebServiceContextAccessor>();

            services.AddSingleton(new HandlerFactory(handlerFactory));

            var options = new XRoadLibOptions();

            configureOptions(options);

            return services.AddSingleton(options);
        }
    }
}