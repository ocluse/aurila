using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

/// <summary>
/// The single gateway to the browser's session history.
/// </summary>
/// <remarks>
/// <para>
/// Aurila treats <c>navigation.entries()</c> as the source of truth for where the user is and where
/// they have been. .NET holds only a projection of it. Consequently this interface is the
/// <em>only</em> thing in the framework permitted to write to session history — there are no calls
/// to <c>history.pushState</c> or <c>history.replaceState</c> anywhere, and adding one reintroduces
/// the drift this design exists to remove.
/// </para>
/// <para>
/// The abstraction also exists so that navigation can be tested without a browser; see
/// <c>InMemoryNavigationLedger</c>.
/// </para>
/// </remarks>
public interface INavigationLedger : IAsyncDisposable
{
    /// <summary>
    /// The host that turns committed navigations into page swaps. Must be set before
    /// <see cref="ActivateAsync"/>.
    /// </summary>
    INavigationDriver? Driver { get; set; }

    /// <summary>
    /// The most recent projection of the entry list. Updated whenever the browser commits a change.
    /// </summary>
    NavSnapshot Snapshot { get; }

    /// <summary>
    /// Raised after <see cref="Snapshot"/> changes.
    /// </summary>
    event EventHandler<NavSnapshot>? SnapshotChanged;

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    /// <summary>
    /// Re-reads the entry list from the browser. Rarely needed — the ledger keeps itself current.
    /// </summary>
    ValueTask<NavSnapshot> RefreshAsync();

    /// <summary>
    /// Navigates to <paramref name="url"/>, pushing or replacing according to
    /// <see cref="NavigateOptions.History"/>.
    /// </summary>
    ValueTask<NavResult> NavigateAsync(string url, NavigateOptions? options = null);

    /// <summary>
    /// Traverses to the entry with the given key, wherever it sits in the list.
    /// </summary>
    /// <remarks>
    /// This is what makes reusable ("single") pages possible: returning to a page that already
    /// exists in history travels to its entry rather than stacking a duplicate.
    /// </remarks>
    ValueTask<NavResult> TraverseToAsync(string entryKey, object? info = null);

    ValueTask<NavResult> BackAsync(object? info = null);

    ValueTask<NavResult> ForwardAsync(object? info = null);

    /// <summary>
    /// Replaces the current entry's state without creating an entry, changing the URL, or firing a
    /// navigate event. This is how page state is persisted.
    /// </summary>
    ValueTask UpdateStateAsync(object? state);

    /// <summary>
    /// Starts intercepting navigations. Until this is called the browser handles navigation itself,
    /// which keeps the window between the app loading and the host being ready from misbehaving.
    /// </summary>
    ValueTask ActivateAsync();

    /// <summary>
    /// Declares whether anything might refuse to leave the current page.
    /// </summary>
    /// <remarks>
    /// When armed, navigations are held back and replayed after confirmation rather than committed
    /// and undone, so a refusal never leaves a wrong URL in the address bar.
    /// </remarks>
    ValueTask SetGuardArmedAsync(bool armed);
}
