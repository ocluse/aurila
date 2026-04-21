using Aurila.Components.Modals;

namespace Aurila.Contracts.Navigation;

public interface INavigator
{
    event EventHandler<NavigatedEventArgs> Navigated;

    void Navigate<TPage>(object? data = null) where TPage : IPage;

    void Navigate(Type pageType, object? data = null);

    void Replace<TPage>(object? data = null) where TPage : IPage;

    void Replace(Type pageType, object? data = null);

    void Navigate(string route);

    void Replace(string route);

    void GoBack();
}