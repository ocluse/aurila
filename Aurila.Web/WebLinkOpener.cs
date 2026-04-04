using Microsoft.JSInterop;

namespace Aurila.Web;

internal sealed class WebLinkOpener(IJSRuntime jsRuntime) : ILinkOpener
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Aurila.Web/aurila.web.js").AsTask());

    public async ValueTask OpenInNewTabAsync(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("openLinkInNewTab", url);
    }
}
