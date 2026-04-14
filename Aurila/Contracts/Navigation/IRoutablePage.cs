namespace Aurila.Contracts.Navigation;

public interface IRoutablePage : IPage
{
    RouteInfo GetRouteInfo();
}
