namespace Aurila.Models.Navigation;

internal static class QueryString
{
    public static string Merge(string route, IReadOnlyDictionary<string, string?> parameters)
    {
        Split(route, out string path, out string query, out string fragment);

        var values = Parse(query);

        foreach (var (key, value) in parameters)
        {
            if (value is null)
            {
                values.Remove(key);
            }
            else
            {
                values[key] = value;
            }
        }

        string rebuilt = string.Join('&', values.Select(v =>
            $"{Uri.EscapeDataString(v.Key)}={Uri.EscapeDataString(v.Value)}"));

        return path
            + (rebuilt.Length == 0 ? string.Empty : "?" + rebuilt)
            + fragment;
    }

    private static void Split(string route, out string path, out string query, out string fragment)
    {
        fragment = string.Empty;
        query = string.Empty;
        path = route;

        int hash = path.IndexOf('#');

        if (hash >= 0)
        {
            fragment = path[hash..];
            path = path[..hash];
        }

        int mark = path.IndexOf('?');

        if (mark >= 0)
        {
            query = path[(mark + 1)..];
            path = path[..mark];
        }
    }

    private static Dictionary<string, string> Parse(string query)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);

            values[Uri.UnescapeDataString(parts[0])] =
                parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        return values;
    }
}
