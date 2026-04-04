using Aurila.Contracts.Navigation;
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
        return services;
    }
}
