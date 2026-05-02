using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Microsoft.Extensions.Options;

namespace Aurila.Services.Navigation;

internal sealed class RouteRegistry : IRouteRegistry
{
    private readonly AurilaRoutingOptions _options;

    private readonly List<(RouteTemplate Template, RouteDefinition Definition)> _routes;

    public RouteRegistry(IOptions<AurilaRoutingOptions> options)
    {
        _options = options.Value;

        _routes = [.. _options.Routes.Select(r => (new RouteTemplate(r.Template), r))];
    }

    public RouteMatch? Match(string path, string? serializedState)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        foreach (var (template, route) in _routes)
        {
            if (template.TryMatch(pathWithoutQuery, out var parameters))
            {
                var data = route.DataFactory?.Invoke(
                    new RouteParameters(parameters, queryParameters),
                    serializedState);

                return new RouteMatch(route.PageType, data);
            }
        }

        return null;
    }

    public RouteParameters ParseRouteParameters(string path, string template)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        var routeTemplate = new RouteTemplate(template);

        var parameters = routeTemplate.TryMatch(pathWithoutQuery, out var dict)
            ? dict
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        return new RouteParameters(parameters, queryParameters);
    }

    public RouteTemplate? GetRouteTemplate(Type pageType)
    {
        var (template, _) = _routes.FirstOrDefault(r => r.Definition.PageType == pageType);
        return template;
    }

    public RouteMatch? GetFallbackRoute()
    {
        if (_options.FallbackRoute == null) return null;

        var data = _options.FallbackRoute.DataFactory?.Invoke(
            new RouteParameters(new Dictionary<string, string>(), new Dictionary<string, string>()),
            null);

        return new RouteMatch(_options.FallbackRoute.PageType, data);
    }

    private static void ParseUrl(
        string path,
        out string pathWithoutQuery,
        out Dictionary<string, string> queryParameters)
    {
        var hashIndex = path.IndexOf('#');
        if (hashIndex >= 0)
            path = path[..hashIndex];

        pathWithoutQuery = path;
        queryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
        {
            pathWithoutQuery = path[..queryIndex];
            var queryString = path[(queryIndex + 1)..];

            foreach (var pair in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kvp = pair.Split('=', 2);

                if (kvp.Length == 2)
                {
                    queryParameters[Uri.UnescapeDataString(kvp[0])] =
                        Uri.UnescapeDataString(kvp[1]);
                }
                else
                {
                    queryParameters[Uri.UnescapeDataString(kvp[0])] = string.Empty;
                }
            }
        }
    }
}

