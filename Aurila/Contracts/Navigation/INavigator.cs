using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

/// <summary>
/// Navigation as pages and controls see it.
/// </summary>
/// <remarks>
/// Every method resolves to a URL and goes out through <see cref="INavigationLedger"/>. Nothing
/// changes what is on screen without first changing the browser's history, which is why the address
/// bar is correct from the first frame and why back and forward behave.
/// </remarks>
public interface INavigator
{
    event EventHandler<NavigatedEventArgs> Navigated;

    Type? CurrentPageType { get; }

    string? CurrentRoute { get; }

    bool CanGoBack { get; }

    bool CanGoForward { get; }

    void Navigate(NavTarget target, object? data = null);

    void Navigate<TPage>(object? data = null, object? routeValues = null) where TPage : IPage;

    void Replace(NavTarget target, object? data = null);

    void Replace<TPage>(object? data = null, object? routeValues = null) where TPage : IPage;

    /// <summary>
    /// Resolves a target to the URL it would navigate to, for use as an anchor's <c>href</c>.
    /// </summary>
    string GetUrl(NavTarget target);

    /// <summary>
    /// Resolves a target to a URL, returning <see langword="false"/> instead of throwing when it
    /// cannot be resolved.
    /// </summary>
    bool TryGetUrl(NavTarget target, out string url);

    /// <summary>
    /// Changes the current entry's URL without changing the page.
    /// </summary>
    void UpdateUrl(string route, NavHistory history = NavHistory.Replace);

    /// <summary>
    /// Adds, replaces or removes query parameters on the current URL. A null value removes the
    /// parameter.
    /// </summary>
    void SetQuery(IReadOnlyDictionary<string, string?> parameters, NavHistory history = NavHistory.Replace);

    void SetQuery(string name, string? value, NavHistory history = NavHistory.Replace);

    void GoBack();

    void GoForward();

    /// <summary>
    /// Writes the current page's state onto its history entry.
    /// </summary>
    /// <remarks>
    /// The framework does this before any navigation it initiates. Call it yourself when the page
    /// changes something worth surviving a reload or a back button, because a traversal started from
    /// browser UI reaches the app only after the entry has already changed — at which point the
    /// outgoing entry can no longer be written to.
    /// </remarks>
    Task PersistStateAsync();

    void AddGuard(INavigationGuard guard);

    void RemoveGuard(INavigationGuard guard);

    /// <summary>
    /// Re-evaluates whether any guard is armed. Call after a guard's <see cref="INavigationGuard.IsArmed"/>
    /// changes.
    /// </summary>
    void RefreshGuards();
}
