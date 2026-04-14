using Aurila.Models;

namespace Aurila.Contracts.Navigation;

public interface INavigationBroker
{
    INavigator? Navigator { get; set; }
    event EventHandler<PageNavigatedEventArgs>? Navigated;
    void NotifyNavigated(object? sender, PageNavigatedEventArgs args);
}
