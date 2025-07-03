using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Services;
public sealed class AurilaJSInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/Aurila/aurila.js").AsTask());

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

    public async Task<IJSObjectReference> CreateObjectAsync(string className, params object[] args)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<IJSObjectReference>($"create{className}", args);
    }

    #region Scroll
    public async Task<ElementScrollValues> GetScrollValuesAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<ElementScrollValues>("getScrollValues", elementReference);
    }

    public async Task ScrollToBottomAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToBottom", elementReference);
    }

    public async Task ScrollToTopAsync(ElementReference elementReference)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToTop", elementReference);
    }

    public async Task ScrollToPositionAsync(ElementReference elementReference, double position, bool isVertical)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scrollToPosition", elementReference, position, isVertical);
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
