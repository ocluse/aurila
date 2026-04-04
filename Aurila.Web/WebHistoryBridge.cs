using Microsoft.JSInterop;

namespace Aurila.Web;

internal sealed class WebHistoryBridge(
    IJSRuntime jsRuntime,
    IBackInterceptor backInterceptor) : IBackNavigationBridge, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Aurila.Web/aurila.web.js").AsTask());

    private DotNetObjectReference<WebHistoryBridge>? _dotNetRef;
    private bool _isInitialized;
    private bool _isDisposing;
    private bool _isInterceptionActive;
    private Task? _pendingActivationTask;

    private async ValueTask EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _dotNetRef = DotNetObjectReference.Create(this);

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeWebHistoryBridge", _dotNetRef);
        _isInitialized = true;
    }

    public async ValueTask SetInterceptionActiveAsync(bool active)
    {
        _isInterceptionActive = active;

        if (_isDisposing)
        {
            return;
        }

        try
        {
            await EnsureInitializedAsync();
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setHistoryInterceptionActive", active);
            return;
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
            return;
        }

        if (active)
        {
            ScheduleActivationRetry();
        }
    }

    private void ScheduleActivationRetry()
    {
        if (_pendingActivationTask is { IsCompleted: false })
        {
            return;
        }

        _pendingActivationTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                if (_isDisposing || _isInterceptionActive == false)
                {
                    return;
                }

                try
                {
                    await EnsureInitializedAsync();
                    var module = await _moduleTask.Value;
                    await module.InvokeVoidAsync("setHistoryInterceptionActive", true);
                    return;
                }
                catch (InvalidOperationException)
                {
                }
                catch (JSDisconnectedException)
                {
                    return;
                }

                await Task.Delay(100);
            }
        });
    }

    [JSInvokable]
    public Task<bool> OnHistoryBackRequested()
    {
        if (_isDisposing || _isInterceptionActive == false)
        {
            return Task.FromResult(false);
        }

        var handled = backInterceptor.OnBackButtonPressed();
        return Task.FromResult(handled);
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposing = true;

        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;

                if (_isInitialized)
                {
                    await module.InvokeVoidAsync("disposeWebHistoryBridge");
                }

                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        finally
        {
            _dotNetRef?.Dispose();
            _dotNetRef = null;
            _isInitialized = false;
            _isDisposing = false;
        }
    }
}
