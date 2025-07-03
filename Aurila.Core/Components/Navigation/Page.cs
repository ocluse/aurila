using Aurila.Contracts.Navigation;

namespace Aurila.Components.Navigation;

public abstract class Page : ComponentBase, IPage
{
    [CascadingParameter]
    public INavigator Nav { get; set; } = null!;

    public virtual void OnNavigatedFrom(NavigationFromEventArgs e)
    {
    }

    public virtual void OnNavigatedTo(NavigationToEventArgs e)
    {

    }

    public virtual Task OnNavigatingFromAsync(NavigationFromEventArgs e)
    {
        return Task.CompletedTask;
    }

    public virtual void OnNavigatingTo(NavigationToEventArgs e)
    {
    }
}