using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

/// <summary>
/// The component that turns a committed navigation into a page swap.
/// </summary>
/// <remarks>
/// Implemented by the navigation host and invoked by <see cref="INavigationLedger"/>. Keeping it an
/// interface is what allows navigation to be driven, and asserted on, without a browser.
/// </remarks>
public interface INavigationDriver
{
    /// <summary>
    /// Runs the page swap for a navigation the browser has already committed.
    /// </summary>
    /// <param name="cancellationToken">Signalled when a newer navigation supersedes this one.</param>
    Task RunAsync(NavigateObservation observation, CancellationToken cancellationToken);

    /// <summary>
    /// Asks the current page whether it may be left. Called before the browser commits, and only
    /// when a page has declared that it might refuse.
    /// </summary>
    ValueTask<bool> ConfirmLeaveAsync(NavigateObservation observation);

    /// <summary>
    /// Writes the current page's state onto its entry, before the document is hidden or unloaded.
    /// </summary>
    Task PersistStateAsync();
}
