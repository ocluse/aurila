using Aurila.Enums.Navigation;

namespace Aurila.Components.Navigation;

/// <summary>
/// Binds a page property to state carried by its history entry.
/// </summary>
/// <remarks>
/// This is everything a navigation knows that the URL does not. A link can only ever carry a URL, so
/// a page must render correctly without any of it; state may make the page faster or restore where
/// the user was, but never determine what it shows.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NavigationStateAttribute(StateScope scope = StateScope.Entry) : Attribute
{
    public StateScope Scope { get; } = scope;

    /// <summary>
    /// The key to store the value under. Defaults to the property name.
    /// </summary>
    public string? Name { get; set; }
}
