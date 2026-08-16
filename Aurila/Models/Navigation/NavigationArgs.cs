using Aurila.Enums.Navigation;
using System.Text.Json;

namespace Aurila.Models.Navigation;

public abstract class NavigationArgs
{
    public required NavIntent Intent { get; init; }
}

public sealed class NavigationFromArgs : NavigationArgs
{
    private volatile bool _cancelled;

    public bool Cancelled => _cancelled;

    /// <summary>
    /// The page being navigated to, or <see langword="null"/> when the destination is not yet known
    /// because the navigation has not been allowed to commit.
    /// </summary>
    public required Type? Destination { get; init; }

    /// <summary>
    /// Refuses the navigation.
    /// </summary>
    /// <remarks>
    /// Honoured only where the browser permits a navigation to be blocked. Traversals started from
    /// browser UI without a recent interaction cannot be blocked, and in that case the page is told
    /// it is leaving rather than asked.
    /// </remarks>
    public void Cancel() => _cancelled = true;
}

public sealed class NavigationToArgs : NavigationArgs
{
    private bool _dataConsumed;

    /// <summary>
    /// The payload handed to <c>Navigate</c>, if this navigation came from one.
    /// </summary>
    /// <remarks>
    /// Ephemeral: it lives only in memory for the duration of this navigation. It is absent when the
    /// user traverses back to the entry, reloads, or opens the URL directly, so a page must be able
    /// to render without it. Anything the page genuinely needs belongs in the URL or in
    /// <see cref="State"/>.
    /// </remarks>
    public object? Data { get; init; }

    /// <summary>
    /// The history entry's persisted state.
    /// </summary>
    /// <remarks>
    /// Survives traversal, reload, session restore and tab duplication.
    /// </remarks>
    public JsonElement? State { get; init; }

    public bool DataConsumed => _dataConsumed;

    public object? ConsumeData()
    {
        if (_dataConsumed)
        {
            throw new InvalidOperationException("Data has already been consumed.");
        }

        _dataConsumed = true;

        return Data;
    }
}
