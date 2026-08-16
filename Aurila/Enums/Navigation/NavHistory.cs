namespace Aurila.Enums.Navigation;

/// <summary>
/// How a navigation should affect the session history list.
/// </summary>
public enum NavHistory
{
    /// <summary>Add a new entry after the current one, truncating anything ahead of it.</summary>
    Push,

    /// <summary>Overwrite the current entry, preserving its key.</summary>
    Replace
}
