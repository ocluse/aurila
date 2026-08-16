using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;
using System.Text.Json;

namespace Aurila.Components.Navigation;

public abstract class AuPage : ComponentBase, IPage, IDisposable
{
    private readonly CancellationTokenSource _disposalCts = new();
    private CancellationTokenSource _navigationCts = new(TimeSpan.Zero);

    protected CancellationToken PageDisposed => _disposalCts.Token;

    protected CancellationToken PageNavigatedFrom => _navigationCts.Token;

    [CascadingParameter]
    public INavigator Nav { get; set; } = null!;

    void IPage.OnNavigatingTo(NavigationToArgs args)
    {
        RenewNavigationCts();

        if (args.State is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null } state)
        {
            RestoreState(state);
        }

        OnNavigatingTo(args);
    }

    private IDictionary<string, object?>? _scratch;

    /// <summary>
    /// Storage for this page's state that cannot be serialized onto its history entry.
    /// </summary>
    /// <remarks>
    /// Held in memory and tied to this page's history entry, so a value is present on a warm
    /// traversal back to it and absent after a reload, a restored session or a duplicated tab. Treat
    /// a miss as the normal case and rebuild from the URL and the entry state.
    /// </remarks>
    protected IDictionary<string, object?> Scratch => _scratch ??= new Dictionary<string, object?>();

    internal void AttachScratch(IDictionary<string, object?> scratch) => _scratch = scratch;

    internal object? CaptureStateForHost() => CaptureState();

    /// <summary>
    /// Produces the state to store on this page's history entry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value must be JSON serializable and should be small — the browser persists it to disk, so
    /// it survives a reload, a restored session and a duplicated tab, and it is not a place for
    /// secrets or for large object graphs.
    /// </para>
    /// <para>
    /// For anything that cannot be serialized, use <see cref="INavigator.Scratch"/> and keep only a
    /// key here; a miss after a cold start is the expected case and should be rebuilt from the URL.
    /// </para>
    /// </remarks>
    protected virtual object? CaptureState() => null;

    /// <summary>
    /// Restores state previously produced by <see cref="CaptureState"/>. Called before
    /// <see cref="OnNavigatingTo"/> whenever the entry carries state.
    /// </summary>
    protected virtual void RestoreState(JsonElement state) { }

    /// <summary>
    /// Writes this page's state onto its history entry now.
    /// </summary>
    protected Task PersistStateAsync() => Nav.PersistStateAsync();

    void IPage.OnNavigatedTo(NavigationToArgs args)
        => OnNavigatedTo(args);

    Task IPage.OnNavigatingFromAsync(NavigationFromArgs args)
        => OnNavigatingFromAsync(args);

    void IPage.OnNavigatedFrom(NavigationFromArgs args)
    {
        try
        {
            _navigationCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        OnNavigatedFrom(args);
    }

    /// <summary>
    /// Re-renders the page. Called by the framework when a two-way query parameter changes from the
    /// URL, which Blazor cannot see because a holder is not a component parameter.
    /// </summary>
    public void NotifyStateChanged() => InvokeAsync(StateHasChanged);

    protected virtual void OnNavigatingTo(NavigationToArgs args) { }

    protected virtual void OnNavigatedFrom(NavigationFromArgs args) { }

    protected virtual void OnNavigatedTo(NavigationToArgs args) { }

    protected virtual Task OnNavigatingFromAsync(NavigationFromArgs args)
        => Task.CompletedTask;

    private void RenewNavigationCts()
    {
        if (!_navigationCts.IsCancellationRequested)
        {
            _navigationCts.Cancel();
        }

        _navigationCts.Dispose();

        //Create a new CTS for the next navigation:
        _navigationCts = CancellationTokenSource.CreateLinkedTokenSource(_disposalCts.Token);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        _disposalCts.Cancel();
        _disposalCts.Dispose();

        _navigationCts.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}