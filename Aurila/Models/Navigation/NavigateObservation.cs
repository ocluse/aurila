using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// A navigate event as reported by the browser, before Aurila has done anything about it.
/// </summary>
public sealed record NavigateObservation
{
    public required NavKind Kind { get; init; }

    public required string DestinationUrl { get; init; }

    /// <summary>App-relative path, or <c>null</c> when the destination is not an app URL.</summary>
    public string? DestinationPath { get; init; }

    /// <summary>The key of the entry being travelled to. Only meaningful for <see cref="NavKind.Traverse"/>.</summary>
    public string? DestinationKey { get; init; }

    /// <summary><c>-1</c> when the destination is a new entry rather than an existing one.</summary>
    public required int DestinationIndex { get; init; }

    /// <summary>
    /// Whether the navigation can be turned into a same-document one. False for cross-origin
    /// destinations, downloads, and entries that predate the app.
    /// </summary>
    public required bool CanIntercept { get; init; }

    /// <summary>
    /// Whether the navigation can be blocked outright. For traversals the platform only allows this
    /// in the top window, same-document, and either not user-initiated or with a consumable user
    /// activation — this flag is the practical meaning of "cancel where it is possible".
    /// </summary>
    public required bool Cancelable { get; init; }

    public required bool UserInitiated { get; init; }

    public required bool HashChange { get; init; }

    /// <summary>
    /// The payload passed to the navigation, as delivered by the platform.
    /// </summary>
    public System.Text.Json.JsonElement? Info { get; init; }

    /// <summary>
    /// Aurila's reading of the navigation, relative to where the user currently is.
    /// </summary>
    public NavIntent ResolveIntent(int currentIndex)
    {
        if (Kind != NavKind.Traverse)
        {
            return Kind switch
            {
                NavKind.Reload => NavIntent.Reload,
                NavKind.Replace => NavIntent.Replace,
                _ => NavIntent.Push
            };
        }

        int offset = DestinationIndex - currentIndex;

        return offset switch
        {
            -1 => NavIntent.Back,
            1 => NavIntent.Forward,
            _ => NavIntent.Jump
        };
    }
}
