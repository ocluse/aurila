using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace Aurila.Services.Navigation;

internal sealed class JsNavigationLedger(
    AurilaJSInterop jsInterop,
    ILogger<JsNavigationLedger> logger) : INavigationLedger
{
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _inFlight = new();
    private readonly ConcurrentDictionary<int, byte> _aborted = new();

    private IJSObjectReference? _jsObject;
    private DotNetObjectReference<JsNavigationLedger>? _selfRef;
    private NavSnapshot _snapshot = NavSnapshot.Empty;
    private bool _disposed;

    public INavigationDriver? Driver { get; set; }

    public NavSnapshot Snapshot => _snapshot;

    public event EventHandler<NavSnapshot>? SnapshotChanged;

    public bool CanGoBack => _snapshot.CanGoBack;

    public bool CanGoForward => _snapshot.CanGoForward;

    public async ValueTask InitializeAsync()
    {
        if (_jsObject is not null)
        {
            return;
        }

        _selfRef = DotNetObjectReference.Create(this);
        _jsObject = await jsInterop.CreateObjectAsync("NavigationLedger", _selfRef);

        await RefreshAsync();
    }

    public async ValueTask ActivateAsync()
        => await Required.InvokeVoidAsync("activate");

    public async ValueTask SetGuardArmedAsync(bool armed)
        => await Required.InvokeVoidAsync("setGuardArmed", armed);

    public async ValueTask<NavSnapshot> RefreshAsync()
    {
        var snapshot = await Required.InvokeAsync<NavSnapshot>("getSnapshot");
        SetSnapshot(snapshot);
        return snapshot;
    }

    public async ValueTask<NavResult> NavigateAsync(string url, NavigateOptions? options = null)
    {
        options ??= NavigateOptions.Push;

        return await Required.InvokeAsync<NavResult>(
            "navigate",
            url,
            options.History == NavHistory.Replace ? "replace" : "push",
            options.State,
            options.Info);
    }

    public async ValueTask<NavResult> TraverseToAsync(string entryKey, object? info = null)
        => await Required.InvokeAsync<NavResult>("traverseTo", entryKey, info);

    public async ValueTask<NavResult> BackAsync(object? info = null)
        => await Required.InvokeAsync<NavResult>("back", info);

    public async ValueTask<NavResult> ForwardAsync(object? info = null)
        => await Required.InvokeAsync<NavResult>("forward", info);

    public async ValueTask UpdateStateAsync(object? state)
        => await Required.InvokeVoidAsync("updateState", state);

    [JSInvokable]
    public Task OnSnapshotChangedAsync(NavSnapshot snapshot)
    {
        SetSnapshot(snapshot);
        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task RunNavigationAsync(NavigationRun run)
    {
        SetSnapshot(run.Snapshot);

        if (Driver is null)
        {
            return;
        }

        var cts = new CancellationTokenSource();
        _inFlight[run.NavigationId] = cts;

        if (_aborted.TryRemove(run.NavigationId, out _))
        {
            cts.Cancel();
        }

        try
        {
            await Driver.RunAsync(run.Observation, cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Navigation to {Path} failed.", run.Observation.DestinationPath);
        }
        finally
        {
            _inFlight.TryRemove(run.NavigationId, out _);
            cts.Dispose();
        }
    }

    [JSInvokable]
    public Task OnNavigationAbortedAsync(int navigationId)
    {
        if (!_inFlight.TryGetValue(navigationId, out var cts))
        {
            _aborted[navigationId] = 0;
            return Task.CompletedTask;
        }

        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        return Task.CompletedTask;
    }

    [JSInvokable]
    public async Task PersistStateAsync()
    {
        if (Driver is null)
        {
            return;
        }

        try
        {
            await Driver.PersistStateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Persisting page state failed.");
        }
    }

    [JSInvokable]
    public async Task<bool> ConfirmLeaveAsync(NavigateObservation observation)
    {
        if (Driver is null)
        {
            return true;
        }

        try
        {
            return await Driver.ConfirmLeaveAsync(observation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A navigation guard threw; the navigation was allowed.");
            return true;
        }
    }

    private IJSObjectReference Required
        => _jsObject ?? throw new InvalidOperationException(
            $"{nameof(JsNavigationLedger)} has not been initialized. " +
            $"Ensure an {nameof(Components.AurilaApp)} component is present at the root of the app.");

    private void SetSnapshot(NavSnapshot snapshot)
    {
        _snapshot = snapshot;

        try
        {
            SnapshotChanged?.Invoke(this, snapshot);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A navigation snapshot subscriber threw.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Driver = null;

        foreach (var cts in _inFlight.Values)
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        _inFlight.Clear();
        _aborted.Clear();

        if (_jsObject is not null)
        {
            try
            {
                await _jsObject.InvokeVoidAsync("dispose");
                await _jsObject.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }

            _jsObject = null;
        }

        _selfRef?.Dispose();
        _selfRef = null;
    }
}
