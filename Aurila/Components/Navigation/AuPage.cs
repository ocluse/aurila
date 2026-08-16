using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;

namespace Aurila.Components.Navigation;

public abstract class AuPage : ComponentBase, IPage, IDisposable
{
    private readonly CancellationTokenSource _disposalCts = new();
    private bool _disposed;

    /// <summary>
    /// Cancelled when the page is torn down, which for an ordinary page is when the user leaves it.
    /// </summary>
    protected CancellationToken PageDisposed => _disposalCts.Token;

    [CascadingParameter]
    public INavigator Nav { get; set; } = null!;

    [Parameter]
    public PageBindingContext? PageBindingContext { get; set; }

    [Inject]
    private PageParametersCache Parameters { get; set; } = null!;

    /// <summary>
    /// Cancelled when the page stops being shown, and renewed when it is shown again. For an ordinary
    /// page that is the same moment as disposal; for a retained page it is the signal to stop working
    /// while it sits off screen.
    /// </summary>
    protected CancellationToken PageHidden => _hiddenCts.Token;

    private CancellationTokenSource _hiddenCts;

    protected AuPage()
    {
        _hiddenCts = CancellationTokenSource.CreateLinkedTokenSource(_disposalCts.Token);
    }

    protected virtual void OnInitializedInternal() { }

    protected sealed override void OnInitialized()
    {
        base.OnInitialized();
        BindQueryParameters();
        OnInitializedInternal();
    }

    protected virtual void OnParametersSetInternal() { }


    protected sealed override void OnParametersSet()
    {
        base.OnParametersSet();
        BindQueryParameters();
        OnParametersSetInternal();
    }

    private void BindQueryParameters()
    {
        if (PageBindingContext is not { } context)
        {
            return;
        }

        Parameters.BindHolders(this, context.RouteParameters, context.Writer);
    }

    internal void Resume()
    {
        if (_hiddenCts.IsCancellationRequested)
        {
            _hiddenCts.Dispose();
            _hiddenCts = CancellationTokenSource.CreateLinkedTokenSource(_disposalCts.Token);
        }

        OnResumed();
    }

    internal void Suspend()
    {
        try
        {
            _hiddenCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        OnSuspended();
    }

    /// <summary>
    /// Writes this page's declared <c>[NavigationState]</c> properties onto its history entry.
    /// </summary>
    /// <remarks>
    /// The framework does this before any navigation it initiates and when the document is hidden.
    /// Call it yourself when the state changes, because a navigation the <em>browser</em> starts
    /// reaches the app only once the entry has already changed, at which point the entry being left
    /// can no longer be written to.
    /// </remarks>
    protected Task PersistStateAsync() => Nav.PersistStateAsync();

    /// <summary>
    /// Called when a retained page is shown again, having stayed alive while the user was elsewhere.
    /// </summary>
    /// <remarks>
    /// Only retained pages receive this, and never on first display — an ordinary page is built anew
    /// each time, so <c>OnInitializedAsync</c> already covers it.
    /// </remarks>
    protected virtual void OnResumed() { }

    /// <summary>
    /// Called when a retained page stops being shown but is kept alive.
    /// </summary>
    protected virtual void OnSuspended() { }

    /// <summary>
    /// Re-renders the page. Called by the framework when a two-way query parameter changes from the
    /// URL, which Blazor cannot see because a holder is not a component parameter.
    /// </summary>
    public void NotifyStateChanged() => InvokeAsync(StateHasChanged);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        _disposed = true;

        _disposalCts.Cancel();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
