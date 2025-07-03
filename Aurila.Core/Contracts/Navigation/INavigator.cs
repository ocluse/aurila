namespace Aurila.Contracts.Navigation;

public interface INavigator
{
    void Navigate<TPage>(object? data = null) where TPage : IPage;

    void Navigate(Type pageType, object? data = null);

    void Replace<TPage>(object? data = null) where TPage : IPage;

    void Replace(Type pageType, object? data = null);

    void SetBusy(bool state);

    void GoBack();
}