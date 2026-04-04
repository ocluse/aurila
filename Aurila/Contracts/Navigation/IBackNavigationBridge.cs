namespace Aurila.Contracts.Navigation;

public interface IBackNavigationBridge
{
    ValueTask SetInterceptionActiveAsync(bool active);
}
