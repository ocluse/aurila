using Aurila.Contracts.Navigation;

namespace Aurila.Services;

internal sealed class NoOpBackNavigationBridge : IBackNavigationBridge
{
    public ValueTask SetInterceptionActiveAsync(bool active) => ValueTask.CompletedTask;
}
