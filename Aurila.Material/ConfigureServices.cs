using Aurila.Material.Appearance;
using Aurila.Material.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurila.Material;

public static class ConfigureServices
{
    public static IServiceCollection AddAurilaMaterial(this IServiceCollection services, string seed)
        => services.AddAurilaMaterial(options => options.Seed = seed);

    public static IServiceCollection AddAurilaMaterial(
        this IServiceCollection services,
        Action<MaterialThemeOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<IAppearanceProvider, MaterialAppearanceProvider>();
        services.TryAddScoped<MaterialThemeService>();
        services.TryAddScoped<MaterialJsInterop>();

        return services;
    }
}
