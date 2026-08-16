using Aurila.Components.Navigation;
using System.Collections.Concurrent;
using System.Reflection;

namespace Aurila.Services.Navigation;

internal static class PageCapabilities
{
    private static readonly ConcurrentDictionary<Type, bool> _mayBlockNavigation = new();

    /// <summary>
    /// Whether a page might refuse to be navigated away from.
    /// </summary>
    /// <remarks>
    /// Blocking has to be decided synchronously, before the browser commits the navigation, but the
    /// answer is asynchronous. This cheap, cached test is what keeps the common case on the fast
    /// path: only pages that actually implement the hook cause a navigation to be held back and
    /// replayed.
    /// </remarks>
    public static bool MayBlockNavigation(Type pageType)
        => _mayBlockNavigation.GetOrAdd(pageType, Compute);

    private static bool Compute(Type pageType)
    {
        if (!typeof(AuPage).IsAssignableFrom(pageType))
        {
            return true;
        }

        var method = pageType.GetMethod(
            "OnNavigatingFromAsync",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return method is not null && method.DeclaringType != typeof(AuPage);
    }
}
