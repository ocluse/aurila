using Aurila.Contracts.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Aurila.Services;

internal class BackInterceptor(IServiceProvider serviceProvider) : IBackInterceptor
{
    private readonly List<IBackReceiver> _receivers = [];
    private IBackNavigationBridge? _backNavigationBridge;

    private IBackNavigationBridge BackNavigationBridge
        => _backNavigationBridge ??= serviceProvider.GetRequiredService<IBackNavigationBridge>();

    public bool OnBackButtonPressed()
    {
        if (_receivers.Count == 0)
        {
            return false;
        }
        var receiver = _receivers[^1];
        return receiver.HandleBackPressed();
    }

    public void RegisterBackReceiver(IBackReceiver receiver)
    {
        bool wasEmpty = _receivers.Count == 0;
        _receivers.Add(receiver);

        if (wasEmpty)
        {
            _ = BackNavigationBridge.SetInterceptionActiveAsync(true);
        }
    }

    public void UnregisterBackReceiver(IBackReceiver receiver)
    {
        bool removed = _receivers.Remove(receiver);
        if (removed && _receivers.Count == 0)
        {
            _ = BackNavigationBridge.SetInterceptionActiveAsync(false);
        }
    }
}
