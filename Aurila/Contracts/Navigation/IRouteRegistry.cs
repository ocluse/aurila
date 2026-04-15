namespace Aurila.Contracts.Navigation;

public interface IRouteRegistry
{
    RouteMatch? Match(string path, string? serializedState);
    RouteMatch? GetFallbackRoute();
    RouteParameters ParseRouteParameters(string path, string template);
}
