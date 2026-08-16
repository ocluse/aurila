using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

/// <summary>
/// Builds URLs from page types and route values.
/// </summary>
/// <remarks>
/// The reverse of <see cref="IRouteRegistry"/>, and the reason links can be real anchors: an
/// <c>&lt;a href&gt;</c> needs a URL at render time, which means the framework has to be able to ask
/// "what is the address of this page?" rather than only "what page is at this address?".
/// </remarks>
public interface IRouteGenerator
{
    /// <summary>
    /// Builds the URL for a page.
    /// </summary>
    /// <param name="pageType">The page to address.</param>
    /// <param name="routeValues">
    /// Values for the template's path parameters. Anything left over becomes a query parameter.
    /// May be an anonymous object or a dictionary.
    /// </param>
    /// <param name="routeName">
    /// Selects between several routes mapped to the same page. When omitted, the first route whose
    /// path parameters are all satisfied is used.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// No route is mapped to the page, or none of its routes has all of its path parameters supplied.
    /// </exception>
    string GetUrl(Type pageType, object? routeValues = null, string? routeName = null);

    /// <summary>
    /// Resolves a target to a URL, whether it names a page or already carries one.
    /// </summary>
    string GetUrl(NavTarget target);

    /// <summary>
    /// Builds the URL for a page, returning <see langword="false"/> instead of throwing when it
    /// cannot be built.
    /// </summary>
    bool TryGetUrl(Type pageType, object? routeValues, out string url);

    bool TryGetUrl(NavTarget target, out string url);
}
