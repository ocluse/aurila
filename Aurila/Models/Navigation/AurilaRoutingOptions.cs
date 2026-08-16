using Aurila.Contracts.Navigation;

namespace Aurila.Models.Navigation;

public class AurilaRoutingOptions
{
    internal List<RouteDefinition> Routes { get; } = [];
    internal RouteDefinition? FallbackRoute { get; set; }

    public void MapRoute<TPage>(
        string template,
        string? name = null,
        Func<RouteParameters, string?, object?>? dataFactory = null) where TPage : IPage
    {
        Routes.Add(new RouteDefinition(typeof(TPage), template, name, dataFactory));
    }

    public void MapFallbackRoute<TPage>(Func<string?, object?>? dataFactory = null) where TPage : IPage
    {
        FallbackRoute = new RouteDefinition(typeof(TPage), string.Empty, null, (args, state) => dataFactory?.Invoke(state));
    }
}

internal record RouteDefinition(
    Type PageType,
    string Template,
    string? Name,
    Func<RouteParameters, string?, object?>? DataFactory);
