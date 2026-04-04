namespace Aurila.Web;

public static class ConfigureServices
{
    public static IServiceCollection AddAurilaWeb(this IServiceCollection services)
    {
        services.AddScoped<IBackNavigationBridge, WebHistoryBridge>();
        return services;
    }
}
