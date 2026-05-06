using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

public interface INavigator
{
    event EventHandler<NavigatedEventArgs> Navigated;

    Type? CurrentPageType { get; }

    void Navigate<TPage>(object? data = null) where TPage : IPage;

    void Navigate(Type pageType, object? data = null);

    void Replace<TPage>(object? data = null) where TPage : IPage;

    void Replace(Type pageType, object? data = null);

    void Navigate(string route);

    void Replace(string route);

    void UpdateUrl(string route);

    void UpdateQueryParameters(IReadOnlyDictionary<string, string?> parameters);

    void GoBack();
}