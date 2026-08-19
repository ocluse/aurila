using Aurila.Fluent.Appearance;
using Aurila.Fluent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurila.Fluent;

public static class ConfigureServices
{
    public static IServiceCollection AddAurilaFluent(this IServiceCollection services, string seed)
        => services.AddAurilaFluent(options => options.Seed = seed);

    public static IServiceCollection AddAurilaFluent(
        this IServiceCollection services,
        Action<FluentThemeOptions>? configure = null)
    {
        services.AddOptions<FluentThemeOptions>();

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<IAppearanceProvider, FluentAppearanceProvider>();
        services.TryAddScoped<FluentThemeService>();
        return services;
    }
}
