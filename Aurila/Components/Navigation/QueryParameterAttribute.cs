namespace Aurila.Components.Navigation;

/// <summary>
/// Binds a page property to a query string parameter.
/// </summary>
/// <remarks>
/// On an ordinary settable property the binding is one way, from the URL. On a
/// <see cref="Models.Navigation.QueryParam{T}"/> it is two way.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class QueryParameterAttribute(string? name = null) : Attribute
{
    /// <summary>
    /// The name to use in the URL. Defaults to the property name.
    /// </summary>
    public string? Name { get; set; } = name;
}
