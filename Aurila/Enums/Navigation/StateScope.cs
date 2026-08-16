namespace Aurila.Enums.Navigation;

/// <summary>
/// Where a page's navigation state is kept.
/// </summary>
public enum StateScope
{
    /// <summary>
    /// Serialized onto the history entry.
    /// </summary>
    /// <remarks>
    /// Survives reload, duplication, traversal etc, as it is persisted by the browser. 
    /// Useful for size-limited, unencrypted data that should be restored when the user returns to the page, like scroll position or a draft.
    /// Not suitable for caching entities, as it will come back stale.
    /// </remarks>
    Entry,

    /// <summary>
    /// Held in memory alongside the history entry. 
    /// </summary>
    /// <remarks>
    /// Should be nullable, because it will be lost on reload or session restore. Useful for caching entities.
    /// </remarks>
    Memory
}
