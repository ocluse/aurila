using Aurila.Contracts.Navigation;

namespace Aurila.Models.Navigation;

public class AurilaRoutingOptions
{
    internal List<RouteDefinition> Routes { get; } = [];
    internal RouteDefinition? FallbackRoute { get; set; }

    /// <summary>
    /// Maps a URL template to a page.
    /// </summary>
    /// <param name="argumentFactory">
    /// Projects the URL into a typed argument, delivered to the page's <c>[RouteArgument]</c>
    /// property. Because it reads only the URL, the result is reproduced on every replay.
    /// </param>
    public void MapRoute<TPage>(
        string template,
        string? name = null,
        Func<RouteParameters, object?>? argumentFactory = null) where TPage : IPage
    {
        Routes.Add(new RouteDefinition(typeof(TPage), template, name, argumentFactory));
    }

    public void MapFallbackRoute<TPage>(Func<RouteParameters, object?>? argumentFactory = null)
        where TPage : IPage
    {
        FallbackRoute = new RouteDefinition(typeof(TPage), string.Empty, null, argumentFactory);
    }
}

internal record RouteDefinition(
    Type PageType,
    string Template,
    string? Name,
    Func<RouteParameters, object?>? ArgumentFactory);
