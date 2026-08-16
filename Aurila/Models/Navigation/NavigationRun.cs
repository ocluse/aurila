namespace Aurila.Models.Navigation;

/// <summary>
/// A navigation the browser has committed and is waiting for the app to render.
/// </summary>
/// <remarks>
/// The snapshot is taken at the moment of the call rather than read back afterwards, so the page
/// swap always sees the entry list exactly as it was when the navigation committed.
/// </remarks>
public sealed record NavigationRun
{
    public required int NavigationId { get; init; }

    public required NavigateObservation Observation { get; init; }

    public required NavSnapshot Snapshot { get; init; }
}
