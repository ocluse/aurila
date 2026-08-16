using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

public interface IRouteRegistry
{
    RouteMatch? Match(string path);
    RouteMatch? GetFallbackRoute();
    RouteTemplate? GetRouteTemplate(Type pageType);
    RouteParameters ParseRouteParameters(string path, string template);
}
