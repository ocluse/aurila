namespace Aurila.Contracts.Components;

using Aurila.Contracts.Navigation;

public interface IAdaptiveNavigationHost
{
    INavigator Navigator { get; }
    Type? CurrentPageType { get; }
    string? CurrentRoute { get; }
    ActiveMatch DefaultMatchMode { get; }
    Func<AdaptiveNavigationItemContext, object?>? DefaultGetData { get; }
    Func<AdaptiveNavigationMatchContext, bool>? DefaultMatch { get; }
}
