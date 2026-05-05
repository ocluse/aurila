using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

public interface IPage
{
    void OnNavigatedTo(NavigationToArgs args);

    void OnNavigatingTo(NavigationToArgs args);

    Task OnNavigatingFromAsync(NavigationFromArgs args);

    void OnNavigatedFrom(NavigationFromArgs args);
}

public interface ISingletonPage : IPage
{
}

public interface IRoutablePage : IPage
{
    RouteInfo GetRouteInfo();
}

public interface INotifyRouteChanged
{
    event EventHandler<RouteInfoChangedEventArgs> RouteChanged;
}