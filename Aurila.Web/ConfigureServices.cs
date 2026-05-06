using Aurila.Contracts;

namespace Aurila.Web;

public static class ConfigureServices
{
    public static IServiceCollection AddAurilaWeb(this IServiceCollection services)
    {
        services.AddScoped<ILinkOpener, WebLinkOpener>();
        return services;
    }
}
