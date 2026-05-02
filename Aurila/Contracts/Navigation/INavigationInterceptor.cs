using Aurila.Enums.Navigation;

namespace Aurila.Contracts.Navigation;

public interface INavigationInterceptor
{
    Task<InterceptionResult> HandleAsync();
}