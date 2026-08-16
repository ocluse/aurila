using Aurila.Enums.Navigation;
using System.Reflection;

namespace Aurila.Models.Navigation;

/// <summary>
/// Where one property of a page gets its value from.
/// </summary>
public enum PageParameterSource
{
    RoutePath,
    Query,
    RouteArgument,
    State
}

public sealed record PageParameterInfo
{
    public required PageParameterSource Source { get; init; }

    /// <summary>
    /// The name the value appears under in the URL or the state bag, which may differ from the
    /// property name.
    /// </summary>
    public required string ExternalName { get; init; }

    public required PropertyInfo Property { get; init; }

    /// <summary>
    /// Whether the property is a <see cref="QueryParam{T}"/>, which is written to after the page
    /// exists rather than supplied as a component parameter.
    /// </summary>
    public required bool IsHolder { get; init; }

    /// <summary>
    /// Where state-sourced values are kept. Meaningless for other sources.
    /// </summary>
    public StateScope Scope { get; init; }

    public Type PropertyType => Property.PropertyType;
}
