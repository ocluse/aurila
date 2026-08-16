using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// The navigation a guard is being asked about.
/// </summary>
public sealed class NavigationLeaveContext
{
    public required NavIntent Intent { get; init; }

    /// <summary>
    /// Where the user is going, or <see langword="null"/> if the destination is outside the app.
    /// </summary>
    public required string? DestinationPath { get; init; }

    /// <summary>
    /// Whether refusing will actually stop the navigation.
    /// </summary>
    /// <remarks>
    /// The browser does not allow every navigation to be blocked — a traversal started from browser
    /// UI without a recent interaction cannot be, because that would let a page trap the user. When
    /// this is <see langword="false"/> the guard is being told the page is leaving rather than asked,
    /// and returning <see langword="false"/> changes nothing. It is still the right moment to persist
    /// state.
    /// </remarks>
    public required bool CanBlock { get; init; }

    public required bool UserInitiated { get; init; }
}
