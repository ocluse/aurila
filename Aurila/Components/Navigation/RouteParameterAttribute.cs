namespace Aurila.Components.Navigation;

/// <summary>
/// Binds a page property to a path segment of its route.
/// </summary>
/// <remarks>
/// Only the path is consulted, never the query string, so a parameter that forms part of the address
/// cannot be overridden by appending to the URL.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RouteParameterAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// The template parameter to bind to. Defaults to the property name.
    /// </summary>
    public string? Name { get; set; } = name;
}
