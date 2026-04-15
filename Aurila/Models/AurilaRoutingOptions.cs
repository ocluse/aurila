using Aurila.Contracts.Navigation;

namespace Aurila.Models;

public class AurilaRoutingOptions
{
    internal List<RouteDefinition> Routes { get; } = [];
    internal RouteDefinition? FallbackRoute { get; set; }

    public void MapRoute<TPage>(string template, Func<RouteParameters, string?, object?>? dataFactory = null) where TPage : IRoutablePage
    {
        Routes.Add(new RouteDefinition(typeof(TPage), template, dataFactory));
    }

    public void MapFallbackRoute<TPage>(Func<string?, object?>? dataFactory = null) where TPage : IRoutablePage
    {
        FallbackRoute = new RouteDefinition(typeof(TPage), string.Empty, (args, state) => dataFactory?.Invoke(state));
    }
}

internal record RouteDefinition(Type PageType, string Template, Func<RouteParameters, string?, object?>? DataFactory);
