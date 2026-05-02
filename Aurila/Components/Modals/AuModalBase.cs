using Aurila.Contracts.Modals;
using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Enums.Navigation;

namespace Aurila.Components.Modals;

public class AuModalBase<TControl> : AuControlBase<TControl>, IModal, INavigationInterceptor, IAsyncDisposable
    where TControl : AuModalBase<TControl>
{
    [CascadingParameter]
    public IAurilaContext AurilaContext { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    private bool _isClosing, _isOpening;
    private bool _openAttribute;
    protected ElementReference _dialogRef;
    private CancellationTokenSource? _ctsClosingAnimation, _ctsOpeningAnimation;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "dialog");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddAttribute(3, "aria-modal", "true");

            if (_openAttribute)
            {
                builder.AddAttribute(4, "open");
            }

            builder.AddElementReferenceCapture(5, r => _dialogRef = r);

            builder.OpenElement(6, "div");
            {
                builder.AddAttribute(7, "class", "au-modal__scrim");
                builder.AddAttribute(8, "onclick", EventCallback.Factory.Create(this, RequestDismiss));
            }
            builder.CloseElement();

            builder.OpenElement(9, "div");
            {
                builder.AddAttribute(10, "class", "au-modal__content-area");
                builder.AddContent(11, ChildContent);
            }
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        var newOpen = parameters.GetValueOrDefault<bool>(nameof(Open));
        var updateState = newOpen != Open;

        await base.SetParametersAsync(parameters);

        if (updateState)
        {
            if (newOpen)
            {
                _ = ExecuteOpeningAsync();
            }
            else
            {
                _ = ExecuteClosingAsync();
            }
        }
    }

    private async Task ExecuteOpeningAsync()
    {
        if (_isOpening) return;

        if (_ctsClosingAnimation != null)
        {
            _ctsClosingAnimation.Cancel();
            _ctsClosingAnimation.Dispose();
            _ctsClosingAnimation = null;
        }

        try
        {
            await AurilaContext.RegisterInterceptorAsync(this);

            _ctsOpeningAnimation = new();

            _openAttribute = true;
            _isOpening = true;
            await InvokeAsync(StateHasChanged);
            await PlayOpenAnimationAsync(_ctsOpeningAnimation.Token);
            _isOpening = false;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
        finally
        {
            _ctsOpeningAnimation?.Dispose();
            _ctsOpeningAnimation = null;
        }
    }

    private async Task ExecuteClosingAsync()
    {
        if (_isClosing) return;

        if (_ctsOpeningAnimation != null)
        {
            _ctsOpeningAnimation.Cancel();
            _ctsOpeningAnimation.Dispose();
            _ctsOpeningAnimation = null;
        }

        try
        {
            _ctsClosingAnimation = new();
            _isClosing = true;

            await InvokeAsync(StateHasChanged);
            await PlayCloseAnimationAsync(_ctsClosingAnimation.Token);
            _isClosing = false;
            _openAttribute = false;

            //remove from back interception:
            await AurilaContext.UnregisterReceiverAsync(this);

            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            //do nothing
        }
        finally
        {
            _ctsClosingAnimation?.Dispose();
            _ctsClosingAnimation = null;
        }
    }

    public async Task ShowAsync()
    {
        if (Open || _isOpening) return;
        await OpenChanged.InvokeAsync(true);
    }

    public async Task HideAsync()
    {
        if (!Open || _isClosing) return;
        await OpenChanged.InvokeAsync(false);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-modal");
        builder.AddIf(_isClosing, "au-modal--closing");
        builder.AddIf(_isOpening, "au-modal--opening");
    }

    private Task RequestDismiss() => HideAsync();

    protected virtual async Task PlayCloseAnimationAsync(CancellationToken cancellationToken = default) 
        => await Task.Delay(300, cancellationToken);

    protected virtual async Task PlayOpenAnimationAsync(CancellationToken cancellationToken = default) 
        => await Task.Delay(300, cancellationToken);

    async Task<InterceptionResult> INavigationInterceptor.HandleAsync()
    {
        await HideAsync();
        return InterceptionResult.Handled;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await AurilaContext.UnregisterReceiverAsync(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
