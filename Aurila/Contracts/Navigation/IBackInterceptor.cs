namespace Aurila.Contracts.Navigation;

public interface IBackInterceptor
{
    bool OnBackButtonPressed();
    void RegisterBackReceiver(IBackReceiver receiver);
    void UnregisterBackReceiver(IBackReceiver receiver);
}
