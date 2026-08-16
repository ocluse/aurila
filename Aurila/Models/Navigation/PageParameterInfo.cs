using System.Reflection;

namespace Aurila.Models.Navigation;

/// <summary>
/// How one property of a page is bound to the URL.
/// </summary>
public sealed record PageParameterInfo
{
    /// <summary>
    /// The name the value appears under in the URL, which may differ from the property name.
    /// </summary>
    public required string ExternalName { get; init; }

    public required PropertyInfo Property { get; init; }

    /// <summary>
    /// Whether the property is a <see cref="QueryParam{T}"/> holder, which is written to after the
    /// page exists rather than supplied as a component parameter.
    /// </summary>
    public required bool IsHolder { get; init; }

    public Type PropertyType => Property.PropertyType;
}
