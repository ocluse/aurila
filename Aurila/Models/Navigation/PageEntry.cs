using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using System.Text.Json;

namespace Aurila.Models.Navigation;

internal sealed class PageEntry
{
    public required string EntryKey { get; init; }

    public required Type PageType { get; init; }

    public required bool IsRetained { get; init; }

    public required bool IsReusable { get; init; }

    public required string Path { get; set; }

    public required RouteParameters RouteParameters { get; set; }

    public JsonElement? EntryState { get; set; }

    public Dictionary<string, object?> Scratch { get; } = [];

    /// <summary>
    /// When this page was last shown, used to evict the least recently used retained page.
    /// </summary>
    public long LastShownAt { get; set; }

    public object? Data { get; set; }

    public IPage? Instance { get; set; }

    public PageState State { get; set; } = PageState.None;

    public NavIntent? Intent { get; set; }

    public IPage EnsuredInstance => Instance
        ?? throw new InvalidOperationException($"The page for history entry '{EntryKey}' has not been rendered yet.");

    public static PageEntry Create(NavEntryRef entry, Type pageType, RouteParameters routeParameters)
    {
        bool singleton = typeof(ISingletonPage).IsAssignableFrom(pageType);

        return new PageEntry
        {
            EntryKey = entry.Key,
            PageType = pageType,
            IsRetained = singleton,
            IsReusable = singleton,
            Path = entry.Path ?? "/",
            RouteParameters = routeParameters,
            EntryState = entry.State
        };
    }
}
