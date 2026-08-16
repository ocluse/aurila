using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using System.Text.Json;

namespace Aurila.Models.Navigation;

internal sealed class PageEntry
{
    public required string EntryKey { get; init; }

    public required Type PageType { get; init; }

    public required bool IsRetained { get; init; }

    public required string Path { get; set; }

    public required RouteParameters RouteParameters { get; set; }

    /// <summary>
    /// Handed to the page so it can bind its own two-way query parameters as it initialises.
    /// </summary>
    public PageBindingContext Binding { get; } = new() { RouteParameters = RouteParameters.Empty };

    public JsonElement? EntryState { get; set; }

    /// <summary>
    /// State kept in memory alongside this entry, for values that are not serialized onto it.
    /// </summary>
    public Dictionary<string, object?> MemoryState { get; } = [];

    /// <summary>
    /// The typed argument produced by the route's factory from this entry's URL.
    /// </summary>
    public object? RouteArgument { get; set; }

    /// <summary>
    /// When this page was last shown, used to evict the least recently used retained page.
    /// </summary>
    public long LastShownAt { get; set; }

    public IPage? Instance { get; set; }

    public PageState State { get; set; } = PageState.None;

    public NavIntent? Intent { get; set; }

    public IPage EnsuredInstance => Instance
        ?? throw new InvalidOperationException($"The page for history entry '{EntryKey}' has not been rendered yet.");

    public static PageEntry Create(
        NavEntryRef entry,
        Type pageType,
        RouteParameters routeParameters,
        object? routeArgument)
    {
        bool singleton = typeof(ISingletonPage).IsAssignableFrom(pageType);

        return new PageEntry
        {
            EntryKey = entry.Key,
            PageType = pageType,
            IsRetained = singleton,
            Path = entry.Path ?? "/",
            RouteParameters = routeParameters,
            RouteArgument = routeArgument,
            EntryState = entry.State
        };
    }
}
