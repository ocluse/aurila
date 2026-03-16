using Aurila.Models;
using Aurila.Services;
using Microsoft.JSInterop;

namespace Aurila.Components.Modals;

public class ModalBottomSheet : ModalBase<ModalBottomSheet>, IAsyncDisposable
{
    [Inject]
    AurilaJSInterop JSInterop { get; set; } = null!;

    /// <summary>
    /// Maximum height of the bottom sheet. Defaults to 90vh.
    /// Users can drag upward to expand up to this height.
    /// </summary>
    [Parameter]
    public CssLength MaxHeight { get; set; } = "90vh";

    private IJSObjectReference? _jsObject;
    private ElementReference _contentAreaRef;
    private bool _jsInitialized;
    private DotNetObjectReference<ModalBottomSheet>? _dotNetRef;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-modal-bottom-sheet");
    }

    // Captures the ElementReference on the content-area div so we can pass it to JS.
    protected override void BuildContentAreaExtras(RenderTreeBuilder builder, int seqStart)
    {
        builder.AddElementReferenceCapture(seqStart, r => _contentAreaRef = r);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (Open && !_jsInitialized && _contentAreaRef.Id != null)
        {
            _jsInitialized = true;
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsObject = await JSInterop.CreateObjectAsync(
                "BottomSheet",
                _contentAreaRef,
                _dotNetRef,
                MaxHeight.ToString()
            );
        }
    }

    protected override async Task PlayCloseAnimation()
    {
        if (_jsObject != null)
            await _jsObject.InvokeVoidAsync("slideOut");
        else
            await base.PlayCloseAnimation();
    }

    [JSInvokable]
    public Task HandleDismissed() => HideAsync();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // When Open goes false and the base has finished the close animation,
        // tear down the JS object so it's recreated fresh on next open.
        if (!Open && _jsInitialized)
        {
            await DisposeJSObjectAsync();
        }
    }

    private async ValueTask DisposeJSObjectAsync()
    {
        _jsInitialized = false;
        if (_jsObject != null)
        {
            try { await _jsObject.InvokeVoidAsync("dispose"); } catch { /* ignore if already torn down */ }
            await _jsObject.DisposeAsync();
            _jsObject = null;
        }
        _dotNetRef?.Dispose();
        _dotNetRef = null;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeJSObjectAsync();
        Dispose();
        GC.SuppressFinalize(this);
    }
}
