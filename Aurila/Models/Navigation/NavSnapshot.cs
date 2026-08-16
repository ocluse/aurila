using System.Text.Json;

namespace Aurila.Models.Navigation;

/// <summary>
/// A point-in-time projection of <c>navigation.entries()</c>.
/// </summary>
/// <remarks>
/// This is a read-only view of the browser's session history, which is the single source of truth
/// for Aurila navigation. Nothing in the framework may mutate session history directly; all writes
/// go through <see cref="Contracts.Navigation.INavigationLedger"/>.
/// </remarks>
public sealed record NavSnapshot(IReadOnlyList<NavEntryRef> Entries, int CurrentIndex)
{
    public static NavSnapshot Empty { get; } = new([], -1);

    /// <summary>
    /// The entry the document is currently sitting on, or <c>null</c> if the snapshot is empty.
    /// </summary>
    public NavEntryRef? Current
        => CurrentIndex >= 0 && CurrentIndex < Entries.Count ? Entries[CurrentIndex] : null;

    public bool CanGoBack => CurrentIndex > 0;

    public bool CanGoForward => CurrentIndex >= 0 && CurrentIndex < Entries.Count - 1;

    public NavEntryRef? Find(string entryKey)
        => Entries.FirstOrDefault(e => e.Key == entryKey);
}

/// <summary>
/// A single <c>NavigationHistoryEntry</c>.
/// </summary>
/// <param name="Key">
/// Stable identity of the history <em>slot</em>. Survives replacement, reload and session restore,
/// so it is safe to use as a cache key for a live page — but never as a source of truth, because a
/// duplicated tab shares keys with a document whose in-memory state it does not have.
/// </param>
/// <param name="Id">Identity of this <em>version</em> of the entry. Changes whenever the entry changes.</param>
/// <param name="Index">Position in <c>navigation.entries()</c>.</param>
/// <param name="Url">Absolute URL, or <c>null</c> when the entry is not readable (not same-origin).</param>
/// <param name="Path">
/// The app-relative path (<c>pathname + search + hash</c>, base href stripped), or <c>null</c>
/// when <paramref name="Url"/> is not readable or falls outside the app's base path.
/// </param>
/// <param name="State">The entry's serialized state, if any.</param>
public sealed record NavEntryRef(
    string Key,
    string Id,
    int Index,
    string? Url,
    string? Path,
    JsonElement? State)
{
    /// <summary>
    /// Whether this entry belongs to the Aurila app and can therefore be matched to a page.
    /// </summary>
    public bool IsAppEntry => Path is not null;
}
