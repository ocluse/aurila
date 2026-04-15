using Microsoft.JSInterop;

namespace Aurila.Services;
public sealed class AurilaJSInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<ValueTask<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Aurila/aurila.js"));

    #region Dialog
    public async ValueTask CloseDialogAsync(ElementReference dialog)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("closeDialog", dialog);
    }

    public async ValueTask ShowDialogAsync(ElementReference dialog)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("showDialog", dialog);
    }

    public async ValueTask ShowPopoverAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("showPopover", element);
    }

    public async ValueTask HidePopoverAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("hidePopover", element);
    }
    #endregion

    public async ValueTask<IJSObjectReference> CreateObjectAsync(string className, params object[] args)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<IJSObjectReference>($"create{className}", args);
    }

    #region Scroll
    public async ValueTask<ElementScrollValues> GetScrollValuesAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<ElementScrollValues>("getScrollValues", elementReference);
    }

    public async ValueTask ScrollToBottomAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToBottom", elementReference);
    }

    public async ValueTask ScrollToTopAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToTop", elementReference);
    }

    public async ValueTask ScrollToPositionAsync(ElementReference elementReference, double position, bool isVertical)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToPosition", elementReference, position, isVertical);
    }

    public async ValueTask<bool> IsNearBottomAsync(ElementReference elementReference, int threshold = 100)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>("isNearBottom", elementReference, threshold);
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
