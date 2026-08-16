using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Microsoft.Extensions.Options;

namespace Aurila.Services.Navigation;

internal sealed class RouteRegistry : IRouteRegistry
{
    private readonly AurilaRoutingOptions _options;

    private readonly List<(RouteTemplate Template, RouteDefinition Definition)> _routes;

    private readonly Dictionary<string, RouteTemplate> _templatesByText = [];

    public RouteRegistry(IOptions<AurilaRoutingOptions> options)
    {
        _options = options.Value;

        _routes = [.. _options.Routes.Select(r => (new RouteTemplate(r.Template), r))];

        foreach (var (template, _) in _routes)
        {
            _templatesByText.TryAdd(template.Template, template);
        }
    }

    public RouteMatch? Match(string path)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        foreach (var (template, route) in _routes)
        {
            if (template.TryMatch(pathWithoutQuery, out var parameters))
            {
                var argument = route.ArgumentFactory?.Invoke(
                    new RouteParameters(parameters, queryParameters));

                return new RouteMatch(route.PageType, argument);
            }
        }

        return null;
    }

    public RouteParameters ParseRouteParameters(string path, string template)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        Dictionary<string, string> parameters = new(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(template))
        {
            if (!_templatesByText.TryGetValue(template, out var routeTemplate))
            {
                routeTemplate = new RouteTemplate(template);
                _templatesByText[template] = routeTemplate;
            }

            if (routeTemplate.TryMatch(pathWithoutQuery, out var matched))
            {
                parameters = matched;
            }
        }

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

        var argument = _options.FallbackRoute.ArgumentFactory?.Invoke(
            new RouteParameters(new Dictionary<string, string>(), new Dictionary<string, string>()));

        return new RouteMatch(_options.FallbackRoute.PageType, argument);
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

