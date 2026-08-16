using Microsoft.JSInterop;

namespace Aurila.Services;

/// <summary>
/// Listens for the platform's close request — <c>Esc</c>, the Android back button, or the back
/// gesture — and reports it to the owner.
/// </summary>
/// <remarks>
/// Dismissing a transient surface is not a navigation, so it does not belong in session history.
/// Routing it through the platform's close-request mechanism is what allows the back button to close
/// a dialog without the framework inventing history entries to catch it with.
/// </remarks>
internal sealed class CloseRequestWatcher : IAsyncDisposable
{
    private readonly Func<Task> _onCloseRequested;
    private readonly DotNetObjectReference<CloseRequestWatcher> _selfRef;
    private IJSObjectReference? _jsObject;
    private bool _disposed;

    private CloseRequestWatcher(Func<Task> onCloseRequested)
    {
        _onCloseRequested = onCloseRequested;
        _selfRef = DotNetObjectReference.Create(this);
    }

    public static async Task<CloseRequestWatcher> CreateAsync(AurilaJSInterop jsInterop, Func<Task> onCloseRequested)
    {
        var watcher = new CloseRequestWatcher(onCloseRequested);

        watcher._jsObject = await jsInterop.CreateObjectAsync("CloseRequestWatcher", watcher._selfRef);

        return watcher;
    }

    [JSInvokable]
    public Task OnCloseRequestedAsync() => _disposed ? Task.CompletedTask : _onCloseRequested();

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_jsObject is not null)
        {
            try
            {
                await _jsObject.InvokeVoidAsync("dispose");
                await _jsObject.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }

            _jsObject = null;
        }

        _selfRef.Dispose();
    }
}
