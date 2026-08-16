using Aurila.Contracts.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// Where a navigation is going: either a page and its route values, or a literal URL.
/// </summary>
/// <remarks>
/// One vocabulary for every place a destination is expressed — the <c>To</c> parameter on clickables,
/// <see cref="INavigator.Navigate(NavTarget, object?)"/>, and <see cref="INavigator.GetUrl(NavTarget)"/>.
/// Because a target can always be resolved to a URL before anything happens, a clickable can render a
/// real anchor rather than a button pretending to be one.
/// </remarks>
public readonly record struct NavTarget
{
    private NavTarget(Type? pageType, object? routeValues, string? routeName, string? url)
    {
        PageType = pageType;
        RouteValues = routeValues;
        RouteName = routeName;
        Url = url;
    }

    public Type? PageType { get; }

    public object? RouteValues { get; }

    /// <summary>
    /// Selects between several routes mapped to the same page. When omitted, the first route whose
    /// path parameters are all satisfied is used.
    /// </summary>
    public string? RouteName { get; }

    /// <summary>
    /// The literal URL, when the target was built from one rather than from a page type.
    /// </summary>
    public string? Url { get; }

    public bool IsEmpty => PageType is null && Url is null;

    public static NavTarget To<TPage>(object? routeValues = null, string? routeName = null)
        where TPage : IPage
        => new(typeof(TPage), routeValues, routeName, null);

    public static NavTarget To(Type pageType, object? routeValues = null, string? routeName = null)
        => new(pageType, routeValues, routeName, null);

    public static NavTarget ToUrl(string url) => new(null, null, null, url);

    public static implicit operator NavTarget(string url) => ToUrl(url);

    public override string ToString() => Url ?? PageType?.Name ?? "(empty)";
}
