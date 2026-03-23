using Microsoft.JSInterop;

namespace Aurila.Components.Modals;

public class BottomSheet : ModalBase<BottomSheet>
{
    [Inject]
    AurilaJSInterop JSInterop { get; set; } = null!;

    private IJSObjectReference? _jsObject;
    private bool _jsInitialized;
    //private DotNetObjectReference<ModalBottomSheet>? _dotNetRef;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-bottom-sheet");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _jsInitialized = true;
            //_dotNetRef = DotNetObjectReference.Create(this);
            _jsObject = await JSInterop.CreateObjectAsync(
                "BottomSheet",
                _dialogRef);
        }
    }

    protected override async Task PlayOpenAnimationAsync(CancellationToken cancellationToken = default)
    {
        if (_jsObject != null)
        {
            await _jsObject.InvokeVoidAsync("open");
        }

        await base.PlayOpenAnimationAsync(cancellationToken);
    }

    protected override async Task PlayCloseAnimationAsync(CancellationToken cancellationToken = default)
    {
        if (_jsObject != null)
        {
            await _jsObject.InvokeVoidAsync("close");
        }

        await base.PlayCloseAnimationAsync(cancellationToken);
    }

    [JSInvokable]
    public Task HandleDismissedAsync() => HideAsync();

    private async ValueTask DisposeJSObjectAsync()
    {
        _jsInitialized = false;
        if (_jsObject != null)
        {
            try { await _jsObject.InvokeVoidAsync("dispose"); } catch { /* ignore if already torn down */ }
            await _jsObject.DisposeAsync();
            _jsObject = null;
        }
        //_dotNetRef?.Dispose();
        //_dotNetRef = null;
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await DisposeJSObjectAsync();
        await base.DisposeAsyncCore();
    }
}
