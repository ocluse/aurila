using Microsoft.JSInterop;
using Aurila.Contracts.Navigation;
using Aurila.Models;
using Microsoft.Extensions.DependencyInjection;
using Aurila.Enums;

namespace Aurila.Web;

internal sealed class WebHistoryBridge : IBackNavigationBridge, IAsyncDisposable
{
    public WebHistoryBridge(
        IJSRuntime jsRuntime,
        INavigationBroker navigationBroker,
        IServiceProvider serviceProvider,
        IBackInterceptor backInterceptor)
    {
        _jsRuntime = jsRuntime;
        _navigationBroker = navigationBroker;
        _serviceProvider = serviceProvider;
        _backInterceptor = backInterceptor;
        
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Aurila.Web/aurila.web.js").AsTask());
        
        // Subscribe to routing changes synchronously and immediately
        _navigationBroker.Navigated += NavigationBroker_Navigated;
    }

    private readonly IJSRuntime _jsRuntime;
    private readonly INavigationBroker _navigationBroker;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackInterceptor _backInterceptor;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private DotNetObjectReference<WebHistoryBridge>? _dotNetRef;
    private bool _isInitialized;
    private bool _isDisposing;
    private bool _isInterceptionActive;
    private Task? _pendingActivationTask;
    private IRouteRegistry? _routeRegistry;

    private async ValueTask EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _routeRegistry = _serviceProvider.GetService<IRouteRegistry>();
        _dotNetRef = DotNetObjectReference.Create(this);

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeWebHistoryBridge", _dotNetRef);
        
        _isInitialized = true;

        if (_routeRegistry != null)
        {
            var initialPath = await module.InvokeAsync<string>("getCurrentPath");
            var initialState = await module.InvokeAsync<string>("getCurrentState");
            HandleLocationChanged(initialPath, initialState);
        }
    }

    private void NavigationBroker_Navigated(object? sender, PageNavigatedEventArgs e)
    {
        if (_routeRegistry == null || _isDisposing || e.NavigationType == NavigationType.Pop)
        {
            return;
        }

        if (e.Instance is IRoutablePage routable)
        {
            var routeInfo = routable.GetRouteInfo();
            
            _ = UpdateBrowserHistoryAsync(routeInfo.Route, routeInfo.SerializedState, e.NavigationType == NavigationType.Replace);
        }
    }

    private async Task UpdateBrowserHistoryAsync(string path, string? state, bool replace)
    {
        try
        {
            var module = await _moduleTask.Value;
            if (replace)
            {
                await module.InvokeVoidAsync("replaceState", state, path);
            }
            else
            {
                await module.InvokeVoidAsync("pushState", state, path);
            }
        }
        catch { }
    }

    [JSInvokable]
    public void OnLocationChanged(string path, string? state)
    {
        HandleLocationChanged(path, state);
    }

    private void HandleLocationChanged(string path, string? state)
    {
        if (_routeRegistry == null) return;

        var match = _routeRegistry.Match(path, state);
        if (match != null)
        {
            _navigationBroker.Navigator?.Replace(match.PageType, match.Data);
        }
        else if (_routeRegistry.GetFallbackRoute() is { } fallback)
        {
            _navigationBroker.Navigator?.Replace(fallback.PageType, fallback.Data);
        }
    }

    public async ValueTask SetInterceptionActiveAsync(bool active)
    {
        _isInterceptionActive = active;

        if (_isDisposing)
        {
            return;
        }

        try
        {
            await EnsureInitializedAsync();
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setHistoryInterceptionActive", active);
            return;
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSDisconnectedException)
        {
            return;
        }

        if (active)
        {
            ScheduleActivationRetry();
        }
    }

    private void ScheduleActivationRetry()
    {
        if (_pendingActivationTask is { IsCompleted: false })
        {
            return;
        }

        _pendingActivationTask = Task.Run(async () =>
        {
            for (int i = 0; i < 50; i++)
            {
                if (_isDisposing || _isInterceptionActive == false)
                {
                    return;
                }

                try
                {
                    await EnsureInitializedAsync();
                    var module = await _moduleTask.Value;
                    await module.InvokeVoidAsync("setHistoryInterceptionActive", true);
                    return;
                }
                catch (InvalidOperationException)
                {
                }
                catch (JSDisconnectedException)
                {
                    return;
                }

                await Task.Delay(100);
            }
        });
    }

    [JSInvokable]
    public Task<bool> OnHistoryBackRequested()
    {
        if (_isDisposing || _isInterceptionActive == false)
        {
            return Task.FromResult(false);
        }

        var handled = _backInterceptor.OnBackButtonPressed();
        return Task.FromResult(handled);
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposing = true;

        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;

                if (_isInitialized)
                {
                    await module.InvokeVoidAsync("disposeWebHistoryBridge");
                }

                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        finally
        {
            if (_isInitialized)
            {
                _navigationBroker.Navigated -= NavigationBroker_Navigated;
            }

            _dotNetRef?.Dispose();
            _dotNetRef = null;
            _isInitialized = false;
            _isDisposing = false;
        }
    }
}
