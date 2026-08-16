using Aurila.Models.Navigation;

namespace Aurila.Contracts.Navigation;

public interface IPage
{
    void OnNavigatedTo(NavigationToArgs args);

    void OnNavigatingTo(NavigationToArgs args);

    Task OnNavigatingFromAsync(NavigationFromArgs args);

    void OnNavigatedFrom(NavigationFromArgs args);
}

/// <summary>
/// A page of which there is at most one history entry and one instance.
/// </summary>
/// <remarks>
/// Navigating to a singleton page that already exists in history travels to its entry rather than
/// stacking a second copy, and its instance stays alive while the user is elsewhere. This is what
/// makes a bottom navigation bar behave the way people expect on mobile.
/// </remarks>
public interface ISingletonPage : IPage
{
}
