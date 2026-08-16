using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// Options for a navigation issued through <see cref="Contracts.Navigation.INavigationLedger"/>.
/// </summary>
public sealed record NavigateOptions
{
    public static NavigateOptions Push { get; } = new();

    public static NavigateOptions Replace { get; } = new() { History = NavHistory.Replace };

    /// <summary>
    /// Whether this navigation adds a history entry or overwrites the current one.
    /// </summary>
    public NavHistory History { get; init; } = NavHistory.Push;

    /// <summary>
    /// Durable, serializable state attached to the resulting history entry.
    /// </summary>
    /// <remarks>
    /// State survives traversal, reload, session restore and tab duplication, and is therefore the
    /// only place — along with the URL — where a page may keep anything it needs in order to exist.
    /// Keep it small: it is persisted to disk by the browser. Never put secrets in it.
    /// </remarks>
    public object? State { get; init; }

    /// <summary>
    /// Ephemeral, non-serialized payload describing <em>why</em> this navigation is happening.
    /// </summary>
    /// <remarks>
    /// Info is delivered to the resulting navigate event and then discarded. It is not replayed when
    /// the user traverses back to the entry, so it may only ever optimise a page's construction —
    /// never determine it. See <see cref="State"/>.
    /// </remarks>
    public object? Info { get; init; }
}

/// <summary>
/// The outcome of a navigation request made through the ledger.
/// </summary>
/// <param name="Committed">
/// Whether the navigation committed — i.e. the URL and the history entry are now live. A navigation
/// that was blocked by a guard, or refused by the browser, does not commit.
/// </param>
/// <param name="ErrorName">The DOMException name when the navigation failed, e.g. <c>AbortError</c>.</param>
/// <param name="ErrorMessage">A human-readable description of the failure, if any.</param>
public sealed record NavResult(bool Committed, string? ErrorName = null, string? ErrorMessage = null)
{
    public static NavResult Success { get; } = new(true);

    public bool IsAborted => ErrorName == "AbortError";
}
