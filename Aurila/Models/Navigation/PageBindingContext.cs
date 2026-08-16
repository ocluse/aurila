namespace Aurila.Models.Navigation;

/// <summary>
/// What a page needs in order to wire up its own two-way query parameters. Used by the framework to bind properties marked with <see cref="QueryParameterAttribute"/>.
/// </summary>
public sealed class PageBindingContext
{
    public required RouteParameters RouteParameters { get; set; }

    internal IQueryParamWriter? Writer { get; set; }
}
