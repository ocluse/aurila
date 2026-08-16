namespace Aurila.Components.Navigation;

/// <summary>
/// Binds a page property to the object produced by its route's argument factory.
/// </summary>
/// <remarks>
/// The factory turns a URL into a typed argument, which is how one page can be reached by several
/// shapes of address without reasoning about which of several nullable parameters was supplied:
/// <c>/users/{id}</c> and <c>/users/@{username}</c> can produce different subtypes of the same
/// argument. Because the factory reads only the URL, the value is reproduced on every replay.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RouteArgumentAttribute : Attribute
{
}
