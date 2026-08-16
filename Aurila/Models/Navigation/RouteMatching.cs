using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// Decides whether a destination is the one currently being shown.
/// </summary>
/// <remarks>
/// Comparison is by address rather than by page type, so two entries of the same page with different
/// route values are correctly distinguished.
/// </remarks>
public static class RouteMatching
{
    public static bool IsActive(string? currentRoute, string? target, ActiveMatch mode)
    {
        if (string.IsNullOrEmpty(currentRoute) || string.IsNullOrEmpty(target))
        {
            return false;
        }

        string current = Normalize(currentRoute);
        string candidate = Normalize(target);

        if (string.Equals(current, candidate, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (mode != ActiveMatch.Prefix)
        {
            return false;
        }

        return current.StartsWith(
            candidate.EndsWith('/') ? candidate : candidate + "/",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string route)
    {
        int end = route.AsSpan().IndexOfAny('?', '#');
        string path = end < 0 ? route : route[..end];

        return path.Length > 1 ? path.TrimEnd('/') : path;
    }
}
