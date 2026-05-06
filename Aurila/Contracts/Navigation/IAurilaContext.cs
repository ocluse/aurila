using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Ocluse.LiquidSnow.Data;

namespace Aurila.Contracts.Navigation;

public interface IAurilaContext
{
    IStateFlow<string> CurrentRoute { get; }

    ValueTask ToggleInterceptorAsync(INavigationInterceptor interceptor, bool enabled);

    ValueTask RegisterInterceptorAsync(INavigationInterceptor interceptor, bool enabled = true);

    ValueTask UnregisterReceiverAsync(INavigationInterceptor interceptor);
}

public interface IAurilaNavigationContext : IAurilaContext
{
    ValueTask CompleteNavigationAsync(RouteInfo routeInfo, NavigationType navigationType);

    ValueTask<IReadOnlyList<NavEntry>> GetNavStackAsync();
}