using Aurila.Components.Modals;
using Aurila.Contracts.Navigation;

namespace Aurila.Components.Controls.Internal;

internal sealed class NullNavigator : INavigator
{
    public static NullNavigator Instance { get; } = new();

    private NullNavigator() { }

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public void GoBack() { }

    public void Navigate<TPage>(object? data = null) where TPage : IPage { }

    public void Navigate(Type pageType, object? data = null) { }

    public void Replace<TPage>(object? data = null) where TPage : IPage { }

    public void Replace(Type pageType, object? data = null) { }

    public void Navigate(string route) { }

    public void Replace(string route) { }
}
