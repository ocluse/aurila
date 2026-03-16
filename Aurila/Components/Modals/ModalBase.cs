using Aurila.Contracts.Components;
using Aurila.Contracts.Navigation;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Modals;

public class ModalBase<TControl> : ControlBase<TControl>, IModal, IBackReceiver, IDisposable
    where TControl : ModalBase<TControl>
{
    [Inject]
    IBackInterceptor BackInterceptor { get; set; } = null!;

    [Inject]
    ModalHostService HostService { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Controls whether the modal is visible. Use @bind-Open for two-way binding.
    /// When the user attempts to dismiss the modal (backdrop click, back button, etc.),
    /// OpenChanged is invoked with false. The parent decides whether to actually close
    /// by updating — or not updating — their bound state variable.
    /// </summary>
    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    private bool _isClosing = false;
    private bool _prevOpen = false;
    private bool _disposed;
    private ModalRegistration? _registration;

    protected override async Task OnParametersSetAsync()
    {
        bool opening = Open && !_prevOpen;
        bool closing = !Open && _prevOpen;

        if (opening)
        {
            _isClosing = false;
            _prevOpen = true;
            BackInterceptor.RegisterBackReceiver(this);
            // Register our overlay fragment with the host — BuildOverlay closes over
            // 'this' so re-renders of the host always get fresh state from this instance.
            _registration = HostService.Register(BuildOverlay);
        }
        else if (_prevOpen && !closing)
        {
            // Already open, not transitioning — a parameter changed (e.g. ChildContent
            // updated by the parent). Notify the host so it re-invokes the fragment and
            // the overlay reflects the new state.
            HostService.NotifyChanged();
        }
        else if (closing)
        {
            await RunCloseAnimation();
            _prevOpen = false;
        }
    }

    // ModalBase emits nothing at its declaration site — all rendering happens inside
    // ModalHost via the registered fragment.
    protected override void BuildRenderTree(RenderTreeBuilder builder) { }

    // Builds the actual overlay markup. Called by ModalHost each time it re-renders,
    // via the RenderFragment registered in ModalHostService.
    private void BuildOverlay(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddAttribute(2, "role", "dialog");
            builder.AddAttribute(3, "aria-modal", "true");

            // Scrim — the semi-transparent backdrop. Clicking it requests dismissal.
            builder.OpenElement(4, "div");
            {
                builder.AddAttribute(5, "class", "au-modal__scrim");
                builder.AddAttribute(6, "onclick", EventCallback.Factory.Create(this, RequestDismiss));
            }
            builder.CloseElement();

            // Content area — stop propagation so clicks inside don't bubble to the scrim.
            builder.OpenElement(7, "div");
            {
                builder.AddAttribute(8, "class", "au-modal__content-area");
                builder.AddEventStopPropagationAttribute(9, "onclick", true);
                builder.AddContent(10, ChildContent);
            }
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    /// <summary>
    /// Requests the modal to open by invoking OpenChanged with true.
    /// The modal opens only if the parent updates the bound Open parameter.
    /// </summary>
    public async Task ShowAsync()
    {
        if (Open) return;
        await OpenChanged.InvokeAsync(true);
    }

    /// <summary>
    /// Requests the modal to close by invoking OpenChanged with false.
    /// The modal closes only if the parent updates the bound Open parameter.
    /// </summary>
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
    }

    private Task RequestDismiss() => HideAsync();

    // Plays the CSS exit animation then removes the overlay from the host.
    // Called from OnParametersSetAsync when Open transitions true → false.
    private async Task RunCloseAnimation()
    {
        _isClosing = true;
        // Notify the host to re-render — the fragment re-runs BuildOverlay with
        // _isClosing=true, applying the au-modal--closing class for the CSS animation.
        HostService.NotifyChanged();

        await Task.Delay(300);

        BackInterceptor.UnregisterBackReceiver(this);
        _isClosing = false;

        if (_registration != null)
        {
            HostService.Unregister(_registration);
            _registration = null;
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
                // If disposed while open (e.g. page navigation), remove from host immediately
                // without animation so the overlay doesn't linger.
                if (_registration != null)
                {
                    HostService.Unregister(_registration);
                    _registration = null;
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
