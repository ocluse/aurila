using Aurila.Contracts.Navigation;
using Microsoft.JSInterop;

namespace Aurila.Services;

internal sealed class BackInterceptor : IBackInterceptor, IAsyncDisposable
{
    private record BackReceiverHandle
    {
        public required IBackReceiver Receiver { get; init; }

        public bool Enabled { get; set; }
    }

    private readonly List<BackReceiverHandle> _receivers = [];

    private readonly Lazy<ValueTask<IJSObjectReference>> _moduleTask;
    private readonly DotNetObjectReference<BackInterceptor> _dotNetRef;

    public BackInterceptor(AurilaJSInterop aurilaJS)
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _moduleTask = new Lazy<ValueTask<IJSObjectReference>>(()
            => aurilaJS.CreateObjectAsync("BackInterceptor", _dotNetRef));
    }

    public bool OnBackButtonPressed()
    {
        if (_receivers.Count == 0)
        {
            return false;
        }
        var receiver = _receivers.FindLast(r => r.Enabled)?.Receiver;
        if (receiver == null)
        {
            return false;
        }
        return receiver.HandleBackPressed();
    }

    private async ValueTask EnsureInitializedAsync()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initialize");
    }

    public async ValueTask RegisterBackReceiverAsync(IBackReceiver receiver)
    {
        await EnsureInitializedAsync();
        _receivers.Add(new BackReceiverHandle { Receiver = receiver, Enabled = true });
    }

    public async ValueTask SetWindowLocationAsync(string url)
    {
        await EnsureInitializedAsync();
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setWindowLocation", url);
    }

    public void UnregisterBackReceiver(IBackReceiver receiver)
    {
        _receivers.RemoveAll(r => r.Receiver == receiver);
    }

    [JSInvokable]
    public bool HandlePopStateAsync()
    {
        return OnBackButtonPressed();
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("cleanup");
            await module.DisposeAsync();
        }
    }

    public void Enable(IBackReceiver receiver)
    {
        var handle = _receivers.Find(r => r.Receiver == receiver);
        handle?.Enabled = true;
    }

    public void Disable(IBackReceiver receiver)
    {
        var handle = _receivers.Find(r => r.Receiver == receiver);
        handle?.Enabled = false;
    }

    public async ValueTask<string> GetCurrentLocationAsync()
    {
        await EnsureInitializedAsync();
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("getWindowLocation");
    }
}
