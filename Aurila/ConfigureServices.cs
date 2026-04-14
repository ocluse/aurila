using Aurila.Contracts.Navigation;
using Aurila.Models;
using Aurila.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurila;

public static class ConfigureServices
{
    public static IServiceCollection AddAurila(this IServiceCollection services)
    {
        services.TryAddScoped<AurilaJSInterop>();
        services.TryAddScoped<IBackInterceptor, BackInterceptor>();
        services.TryAddScoped<IBackNavigationBridge, NoOpBackNavigationBridge>();
        services.TryAddScoped<IImageLoader, DefaultImageLoader>();
        services.TryAddScoped<INavigationBroker, NavigationBroker>();
        return services;
    }

    public static IServiceCollection AddAurilaRouting(this IServiceCollection services, Action<AurilaRoutingOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.TryAddSingleton<IRouteRegistry, RouteRegistry>();
        return services;
    }
}

