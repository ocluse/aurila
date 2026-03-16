using Aurila.Contracts.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Aurila;

public static class ConfigureServices
{
    public static IServiceCollection AddAurila(this IServiceCollection services)
    {
        services.AddScoped<AurilaJSInterop>();
        services.AddSingleton<IBackInterceptor, BackInterceptor>();
        services.AddSingleton<ModalHostService>();
        services.AddScoped<IImageLoader, DefaultImageLoader>();
        return services;
    }
}
