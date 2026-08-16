using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Services.Navigation;

/// <summary>
/// The .NET projection of <c>navigation.entries()</c>: a cache of live pages keyed by history entry.
/// </summary>
/// <remarks>
/// <para>
/// The store never decides where the user is; it only answers what page belongs to an entry. A page
/// is reused when its entry key and path still match, and rebuilt from the entry's URL and state
/// otherwise. Deep links, reloads, duplicated tabs, restored sessions and cold traversals therefore
/// all take the same path, because nothing a page needs lives only in memory.
/// </para>
/// </remarks>
internal sealed class PageStore(IRouteRegistry routeRegistry)
{
    private readonly Dictionary<string, PageEntry> _live = new(StringComparer.Ordinal);
    private const int MaxCachedPaths = 256;

    private readonly Dictionary<string, Type?> _pageTypeByPath = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<PageEntry> Live => _live.Values;

    /// <summary>
    /// The page a URL would resolve to, without creating anything.
    /// </summary>
    public Type? PeekPageType(string path, Type? startPage = null) => ResolvePageType(path, startPage);

    public PageEntry? Find(string entryKey)
        => _live.TryGetValue(entryKey, out var entry) ? entry : null;

    /// <summary>
    /// Returns the page for a history entry, reusing the live instance when the entry still
    /// addresses the same page and rebuilding it from the entry otherwise.
    /// </summary>
    public PageEntry Resolve(NavEntryRef entry, Type? fallbackPageType)
    {
        string path = entry.Path ?? "/";

        var match = routeRegistry.Match(path);

        if (_live.TryGetValue(entry.Key, out var existing))
        {
            if (string.Equals(existing.Path, path, StringComparison.OrdinalIgnoreCase))
            {
                existing.EntryState = entry.State;
                return existing;
            }

            if (match?.PageType == existing.PageType)
            {
                existing.Path = path;
                existing.EntryState = entry.State;
                existing.RouteParameters = ParseParameters(existing.PageType, path);
                existing.RouteArgument = match.Argument;
                return existing;
            }
        }

        var pageType = ResolvePageType(path, fallbackPageType)
            ?? throw new InvalidOperationException(
                $"No page is mapped to '{path}' and no fallback route is configured. " +
                "Map one with MapFallbackRoute<TPage>().");

        var created = PageEntry.Create(entry, pageType, ParseParameters(pageType, path), match?.Argument);

        _live[entry.Key] = created;

        return created;
    }

    /// <summary>
    /// Finds an existing history entry already at <paramref name="path"/>, which is how a reusable
    /// page is travelled to rather than stacked a second time.
    /// </summary>
    public NavEntryRef? FindEntryForPath(NavSnapshot snapshot, string path)
    {
        for (int i = snapshot.Entries.Count - 1; i >= 0; i--)
        {
            var entry = snapshot.Entries[i];

            if (entry.IsAppEntry && string.Equals(entry.Path, path, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>
    /// Drops pages whose history entry no longer exists, together with any page that is neither
    /// retained nor currently on screen.
    /// </summary>
    public void Prune(NavSnapshot snapshot, string? currentEntryKey, int maxRetained)
    {
        var surviving = new HashSet<string>(snapshot.Entries.Select(e => e.Key), StringComparer.Ordinal);

        foreach (var key in _live.Keys.ToList())
        {
            var entry = _live[key];

            if (!surviving.Contains(key))
            {
                Evict(key, entry);
                continue;
            }

            bool keepInstance = entry.IsRetained
                || string.Equals(key, currentEntryKey, StringComparison.Ordinal);

            if (!keepInstance)
            {
                entry.Instance = null;
            }
        }

        EvictOverflow(currentEntryKey, maxRetained);
    }

    /// <summary>
    /// Drops the least recently shown retained pages once there are more of them than allowed.
    /// </summary>
    private void EvictOverflow(string? currentEntryKey, int maxRetained)
    {
        if (maxRetained < 0)
        {
            return;
        }

        var retained = _live
            .Where(p => p.Value.IsRetained
                && p.Value.Instance is not null
                && !string.Equals(p.Key, currentEntryKey, StringComparison.Ordinal))
            .OrderBy(p => p.Value.LastShownAt)
            .ToList();

        bool currentIsRetained = currentEntryKey is not null
            && _live.TryGetValue(currentEntryKey, out var current)
            && current.IsRetained;

        int allowance = Math.Max(maxRetained - (currentIsRetained ? 1 : 0), 0);
        int excess = retained.Count - allowance;

        for (int i = 0; i < excess; i++)
        {
            retained[i].Value.Instance = null;
        }
    }

    private void Evict(string key, PageEntry entry)
    {
        entry.Instance = null;
        entry.MemoryState.Clear();
        _live.Remove(key);
    }

    private Type? ResolvePageType(string path, Type? startPage)
    {
        string cacheKey = WithoutQuery(path);

        if (!_pageTypeByPath.TryGetValue(cacheKey, out var pageType))
        {
            pageType = routeRegistry.Match(path)?.PageType;

            if (_pageTypeByPath.Count >= MaxCachedPaths)
            {
                _pageTypeByPath.Clear();
            }

            _pageTypeByPath[cacheKey] = pageType;
        }

        if (pageType is not null)
        {
            return pageType;
        }

        return startPage is not null && IsRoot(path)
            ? startPage
            : routeRegistry.GetFallbackRoute()?.PageType;
    }

    private static string WithoutQuery(string path)
    {
        int end = path.AsSpan().IndexOfAny('?', '#');

        return end < 0 ? path : path[..end];
    }

    private static bool IsRoot(string path)
    {
        int end = path.AsSpan().IndexOfAny('?', '#');
        var trimmed = end < 0 ? path.AsSpan() : path.AsSpan(0, end);

        return trimmed.IsEmpty || trimmed is "/";
    }

    private RouteParameters ParseParameters(Type pageType, string path)
    {
        var template = routeRegistry.GetRouteTemplate(pageType);

        return routeRegistry.ParseRouteParameters(path, template?.Template ?? string.Empty);
    }
}
