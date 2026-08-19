using Microsoft.JSInterop;

namespace Aurila.Material.Services;

public sealed class MaterialJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<ValueTask<IJSObjectReference>> _module = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Aurila.Material/aurila.material.js"));

    private bool _rippleEnabled;

    public async ValueTask EnableRippleAsync()
    {
        if (_rippleEnabled)
        {
            return;
        }

        _rippleEnabled = true;
        IJSObjectReference module = await _module.Value;
        await module.InvokeVoidAsync("enableRipple");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_module.IsValueCreated)
        {
            return;
        }

        try
        {
            IJSObjectReference module = await _module.Value;
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The circuit is already gone; nothing to release.
        }
    }
}
