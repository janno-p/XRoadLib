using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace XRoadLib.Extensions.AspNetCore;

public static class XRoadLibServiceCollectionExtensions
{
    [UsedImplicitly]
    public static IServiceCollection AddXRoadLib(this IServiceCollection services) =>
        AddXRoadLib(services, _ => {});

    [UsedImplicitly]
    public static IServiceCollection AddXRoadLib(this IServiceCollection services, Action<XRoadLibOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        services.AddRouting();
        services.AddScoped<IWebServiceContextAccessor, WebServiceContextAccessor>();

        var options = new XRoadLibOptions();

        configureOptions(options);

        return services.AddSingleton(options);
    }
}