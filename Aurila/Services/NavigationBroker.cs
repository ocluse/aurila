using Aurila.Contracts.Navigation;
using Aurila.Models;

namespace Aurila.Services;

internal class NavigationBroker : INavigationBroker
{
    public INavigator? Navigator { get; set; }
    public event EventHandler<PageNavigatedEventArgs>? Navigated;

    public void NotifyNavigated(object? sender, PageNavigatedEventArgs args)
    {
        Navigated?.Invoke(sender, args);
    }
}
