using Microsoft.Extensions.DependencyInjection;

namespace Aurila;

public static class ConfigureServices
{
    public static IServiceCollection AddAurila(this IServiceCollection services)
    {
        services.AddSingleton<AurilaJSInterop>();
        services.AddSingleton<BackInterceptor>();
        return services;
    }
}
