using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Text;

namespace Aurila.Services.Navigation;

internal sealed class RouteGenerator(IOptions<AurilaRoutingOptions> options) : IRouteGenerator
{
    private readonly List<(RouteTemplate Template, RouteDefinition Definition)> _routes =
        [.. options.Value.Routes.Select(r => (new RouteTemplate(r.Template), r))];

    public string GetUrl(Type pageType, object? routeValues = null, string? routeName = null)
    {
        var values = ToDictionary(routeValues);
        var match = FindRoute(pageType, routeName, values);

        if (match is not null)
        {
            return Build(match, values);
        }

        var candidates = Candidates(pageType, routeName);

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(routeName is null
                ? $"No route is mapped to '{pageType.FullName}'. Map one with MapRoute<{pageType.Name}>(\"...\")."
                : $"No route named '{routeName}' is mapped to '{pageType.FullName}'.");
        }

        var required = string.Join(", ", candidates[0].Template.Parameters.Select(p => p.Name));

        throw new InvalidOperationException(
            $"Cannot build a URL for '{pageType.FullName}': the route '{candidates[0].Template.Template}' " +
            $"requires [{required}], but " +
            (values.Count == 0
                ? "no route values were supplied."
                : $"only [{string.Join(", ", values.Keys)}] were supplied."));
    }

    private RouteTemplate? FindRoute(Type pageType, string? routeName, IReadOnlyDictionary<string, object?> values)
        => Candidates(pageType, routeName)
            .FirstOrDefault(c => c.Template.Parameters.All(p => values.ContainsKey(p.Name)))
            .Template;

    private List<(RouteTemplate Template, RouteDefinition Definition)> Candidates(Type pageType, string? routeName)
        => [.. _routes
            .Where(r => r.Definition.PageType == pageType)
            .Where(r => routeName is null || string.Equals(r.Definition.Name, routeName, StringComparison.OrdinalIgnoreCase))];

    public string GetUrl(NavTarget target)
    {
        if (target.Url is not null)
        {
            return target.Url;
        }

        if (target.PageType is null)
        {
            throw new InvalidOperationException("The navigation target is empty.");
        }

        return GetUrl(target.PageType, target.RouteValues, target.RouteName);
    }

    public bool TryGetUrl(NavTarget target, out string url)
    {
        if (target.Url is not null)
        {
            url = target.Url;
            return true;
        }

        if (target.PageType is null)
        {
            url = string.Empty;
            return false;
        }

        return TryGetUrl(target.PageType, target.RouteValues, target.RouteName, out url);
    }

    public bool TryGetUrl(Type pageType, object? routeValues, out string url)
        => TryGetUrl(pageType, routeValues, null, out url);

    private bool TryGetUrl(Type pageType, object? routeValues, string? routeName, out string url)
    {
        var values = ToDictionary(routeValues);
        var match = FindRoute(pageType, routeName, values);

        if (match is null)
        {
            url = string.Empty;
            return false;
        }

        url = Build(match, values);
        return true;
    }

    private static string Build(RouteTemplate template, IReadOnlyDictionary<string, object?> values)
    {
        var consumed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var path = new StringBuilder();
        int index = 0;
        string source = template.Template;

        while (index < source.Length)
        {
            int open = source.IndexOf('{', index);

            if (open < 0)
            {
                path.Append(source, index, source.Length - index);
                break;
            }

            path.Append(source, index, open - index);

            int close = source.IndexOf('}', open);

            if (close < 0)
            {
                throw new InvalidOperationException($"Route template '{source}' has an unclosed parameter.");
            }

            string name = source[(open + 1)..close].Split(':', 2)[0];

            path.Append(Uri.EscapeDataString(RouteValueFormatter.Format(values.GetValueOrDefault(name))));
            consumed.Add(name);

            index = close + 1;
        }

        var query = values
            .Where(v => !consumed.Contains(v.Key) && v.Value is not null)
            .Select(v => $"{Uri.EscapeDataString(v.Key)}={Uri.EscapeDataString(RouteValueFormatter.Format(v.Value))}")
            .ToList();

        string result = path.ToString();

        if (!result.StartsWith('/'))
        {
            result = "/" + result;
        }

        return query.Count == 0 ? result : $"{result}?{string.Join('&', query)}";
    }

    private static Dictionary<string, object?> ToDictionary(object? routeValues)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        switch (routeValues)
        {
            case null:
                break;

            case IReadOnlyDictionary<string, object?> typed:
                foreach (var (key, value) in typed)
                {
                    values[key] = value;
                }
                break;

            case IDictionary dictionary:
                foreach (DictionaryEntry entry in dictionary)
                {
                    values[entry.Key.ToString()!] = entry.Value;
                }
                break;

            default:
                foreach (var property in routeValues.GetType().GetProperties())
                {
                    if (property.CanRead)
                    {
                        values[property.Name] = property.GetValue(routeValues);
                    }
                }
                break;
        }

        return values;
    }
}
