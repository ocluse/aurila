using Aurila.Contracts.Components;
using Aurila.Contracts.Navigation;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Modals;
public class ModalBase<TControl> : ControlBase<TControl>, IModal, IBackReceiver, IDisposable
    where TControl : ModalBase<TControl>
{
    [Inject]
    public AurilaJSInterop JSInterop { get; set; } = null!;

    [Inject]
    IBackInterceptor BackInterceptor { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public Func<Task<bool>>? OnDismissingFunc { get; set; }

    private bool _isVisible = false;
    private bool _isClosing = false;
    private ElementReference _dialogElement;
    private bool _disposed;

    public async Task ShowAsync()
    {
        if (_isVisible) return;

        _isVisible = true;
        _isClosing = false;

        BackInterceptor.RegisterBackReceiver(this);

        await InvokeAsync(async () =>
        {
            StateHasChanged();
            await JSInterop.ShowDialogAsync(_dialogElement);
        });
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "dialog");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, HideAsync));
            builder.AddAttribute(3, "onclose", EventCallback.Factory.Create(this, HandleDialogClose));
            builder.AddElementReferenceCapture(4, element => _dialogElement = element);
            if (_isVisible)
            {
                builder.OpenElement(5, "div");
                {
                    //TODO: Look at this implementation, it might not be correct
                    builder.AddAttribute(6, "class", "au-dialog__content-area");
                    builder.AddEventStopPropagationAttribute(7, "onclick", true);
                    builder.AddContent(8, ChildContent);
                }
                builder.CloseElement();
            }
        }
        builder.CloseElement();
    }
    public async Task HideAsync()
    {
        // Prevent hiding if already closing or not visible
        if (_isClosing || !_isVisible) return;

        if (OnDismissingFunc != null)
        {
            bool cancel = await OnDismissingFunc();
            if (cancel)
            {
                return;
            }
        }

        _isClosing = true; // Start the closing process

        await InvokeAsync(StateHasChanged);

        // Wait for the CSS animation to complete
        await Task.Delay(300);

        await JSInterop.CloseDialogAsync(_dialogElement);

       BackInterceptor.UnregisterBackReceiver(this);

        _isVisible = false;
        _isClosing = false;

        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-modal");
        builder.AddIf(_isVisible, "visible")
            .AddIf(_isClosing, "closing");
    }

    private async Task HandleDialogClose()
    {
        if (_isVisible && !_isClosing)
        {
            _isVisible = false;
            _isClosing = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    public bool HandleBackPressed()
    {
        _ = HideAsync();
        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                BackInterceptor.UnregisterBackReceiver(this);
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
