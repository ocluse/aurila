namespace Aurila.Contracts.Navigation;

public interface IBackInterceptor
{
    bool OnBackButtonPressed();
    
    void Enable(IBackReceiver receiver);

    void Disable(IBackReceiver receiver);

    ValueTask RegisterBackReceiverAsync(IBackReceiver receiver);
    
    void UnregisterBackReceiver(IBackReceiver receiver);
    
    ValueTask SetWindowLocationAsync(string url);

    ValueTask<string> GetCurrentLocationAsync();
}
