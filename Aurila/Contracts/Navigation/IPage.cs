namespace Aurila.Contracts.Navigation;

public interface IPage
{
    void OnNavigatedTo(NavigationToEventArgs e);

    void OnNavigatingTo(NavigationToEventArgs e);

    Task OnNavigatingFromAsync(NavigationFromEventArgs e);

    void OnNavigatedFrom(NavigationFromEventArgs e);
}

public interface ISingletonPage : IPage
{
}
