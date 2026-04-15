using Aurila.Contracts.Navigation;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Aurila.Services;

internal sealed partial class RouteRegistry : IRouteRegistry
{
    private readonly AurilaRoutingOptions _options;
    private readonly Dictionary<string, Regex> _routeRegexCache = new(StringComparer.OrdinalIgnoreCase);

    public RouteRegistry(IOptions<AurilaRoutingOptions> options)
    {
        _options = options.Value;

        // Pre-compile and cache regex patterns for extreme performance
        foreach (var route in _options.Routes)
        {
            if (!_routeRegexCache.ContainsKey(route.Template))
            {
                _routeRegexCache[route.Template] = BuildRegexForTemplate(route.Template);
            }
        }
    }

    public RouteMatch? GetFallbackRoute()
    {
        if (_options.FallbackRoute == null) return null;

        var data = _options.FallbackRoute.DataFactory?.Invoke(
            new RouteParameters(new Dictionary<string, string>(), new Dictionary<string, string>()), null);

        return new RouteMatch(_options.FallbackRoute.PageType, data);
    }

    public RouteMatch? Match(string path, string? serializedState)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        foreach (var route in _options.Routes)
        {
            // Fallback to building it if it was added dynamically after initialization
            if (!_routeRegexCache.TryGetValue(route.Template, out var regex))
            {
                regex = BuildRegexForTemplate(route.Template);
                _routeRegexCache[route.Template] = regex;
            }

            var matchRoute = regex.Match(pathWithoutQuery);

            if (matchRoute.Success)
            {
                var dict = ExtractParameters(matchRoute);
                var data = route.DataFactory?.Invoke(new RouteParameters(dict, queryParameters), serializedState);
                return new RouteMatch(route.PageType, data);
            }
        }

        return null;
    }

    public RouteParameters ParseRouteParameters(string path, string template)
    {
        ParseUrl(path, out var pathWithoutQuery, out var queryParameters);

        if (!_routeRegexCache.TryGetValue(template, out var regex))
        {
            regex = BuildRegexForTemplate(template);
            _routeRegexCache[template] = regex;
        }

        var match = regex.Match(pathWithoutQuery);
        var parameters = match.Success
            ? ExtractParameters(match)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        return new RouteParameters(parameters, queryParameters);
    }

    // --- Helper Methods ---

    private static Regex BuildRegexForTemplate(string template)
    {
        // Safely parse the template: Match either tokens '{...}' or literal strings '[^{]+'
        // This ensures characters like '.' or '?' in the template act as literals, not regex wildcards.
        var patternStr = TemplateParserRegex().Replace(template, match =>
        {
            if (match.Groups[1].Success)
            {
                // It's a token (e.g., "{id:int}")
                var token = match.Groups[1].Value;
                var parts = token.Split(':');
                var name = parts[0];
                var constraint = parts.Length > 1 ? parts[1].ToLowerInvariant() : "";

                var regexPart = constraint switch
                {
                    "int" => @"\d+",
                    "guid" => @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
                    "bool" => @"true|false",
                    "date" => @"\d{4}-\d{2}-\d{2}",
                    "time" => @"\d{2}(:|%3[aA])\d{2}((:|%3[aA])\d{2})?(?:\.\d+)?",
                    "datetime" => @"\d{4}-\d{2}-\d{2}[T ]\d{2}(:|%3[aA])\d{2}[^/]*",
                    _ => @"[^/]+"
                };

                return $"(?<{name}>{regexPart})";
            }

            // It's a literal path string (e.g., "/api/v1.0/") - Escape it!
            return Regex.Escape(match.Value);
        });

        // Add start/end anchors and allow an optional trailing slash
        return new Regex($"^{patternStr}/?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private static void ParseUrl(string path, out string pathWithoutQuery, out Dictionary<string, string> queryParameters)
    {
        // 1. Strip fragments (#) safely to avoid breaking query strings
        var hashIndex = path.IndexOf('#');
        if (hashIndex >= 0)
        {
            path = path[..hashIndex];
        }

        pathWithoutQuery = path;
        queryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 2. Extract Query Parameters
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
                    queryParameters[Uri.UnescapeDataString(kvp[0])] = Uri.UnescapeDataString(kvp[1]);
                }
                else if (kvp.Length == 1)
                {
                    queryParameters[Uri.UnescapeDataString(kvp[0])] = string.Empty;
                }
            }
        }
    }

    private static Dictionary<string, string> ExtractParameters(Match match)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Group group in match.Groups.Cast<Group>())
        {
            // Ignore index '0' (the full match) and standard numeric groups to only grab explicitly named route variables
            if (group.Success && !int.TryParse(group.Name, out _))
            {
                dict[group.Name] = Uri.UnescapeDataString(group.Value);
            }
        }
        return dict;
    }

    [GeneratedRegex(@"\{([^}]+)\}|([^{]+)")]
    private static partial Regex TemplateParserRegex();
}
