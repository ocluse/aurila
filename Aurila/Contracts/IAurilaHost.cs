namespace Aurila.Contracts;

public interface IAurilaHost
{
    event Action<object?>? IntentReceived;

    object? GetLaunchIntent();

    void RequestExit();

    Task OpenLinkAsync(Uri link);
}
