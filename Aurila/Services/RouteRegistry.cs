using Aurila.Contracts.Navigation;
using Aurila.Models;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Aurila.Services;

internal sealed partial class RouteRegistry(IOptions<AurilaRoutingOptions> options) : IRouteRegistry
{
    private readonly AurilaRoutingOptions _options = options.Value;

    public RouteMatch? GetFallbackRoute()
    {
        if (_options.FallbackRoute == null) return null;

        var data = _options.FallbackRoute.DataFactory?.Invoke(new RouteParameters(new Dictionary<string, string>()), null);
        return new RouteMatch(_options.FallbackRoute.PageType, data);
    }

    public RouteMatch? Match(string path, string? serializedState)
    {
        // Simple token matching with constraints
        foreach (var route in _options.Routes)
        {
            var pattern = "^" + RouteTokenMatchRegex().Replace(route.Template, match =>
            {
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
            }) + "$";

            var matchRoute = Regex.Match(path, pattern, RegexOptions.IgnoreCase);

            if (matchRoute.Success)
            {
                var dict = new Dictionary<string, string>();
                foreach (Group group in matchRoute.Groups)
                {
                    if (group.Name != "0")
                    {
                        dict[group.Name] = group.Value;
                    }
                }

                var data = route.DataFactory?.Invoke(new RouteParameters(dict), serializedState);
                return new RouteMatch(route.PageType, data);
            }
        }

        return null;
    }

    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex RouteTokenMatchRegex();
}
