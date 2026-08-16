namespace Aurila.Enums.Navigation;

/// <summary>
/// Aurila's reading of what a navigation means, derived from <see cref="NavKind"/> and the
/// destination's position relative to the current entry.
/// </summary>
public enum NavIntent
{
    /// <summary>A new entry was appended.</summary>
    Push,

    /// <summary>The current entry was overwritten by a different page.</summary>
    Replace,

    /// <summary>A traversal to the immediately preceding entry.</summary>
    Back,

    /// <summary>A traversal to the immediately following entry.</summary>
    Forward,

    /// <summary>A traversal of more than one entry in either direction.</summary>
    Jump,

    /// <summary>The current entry was reloaded.</summary>
    Reload,

    /// <summary>
    /// The current entry's URL changed but the page did not — a query string or path parameter
    /// update. The page is re-bound in place; no transition and no lifecycle teardown.
    /// </summary>
    Rebind
}
