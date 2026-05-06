using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Microsoft.JSInterop;
using Ocluse.LiquidSnow.Data;

namespace Aurila.Components;

public sealed class AurilaApp(AurilaJSInterop jsInterop) : ComponentBase, IAurilaNavigationContext, IAsyncDisposable
{
    private record NavigationInterceptorHandle
    {
        public required INavigationInterceptor Interceptor { get; init; }

        public bool Enabled { get; set; }
    }

    private readonly List<NavigationInterceptorHandle> _navigationInterceptors = [];
    private IJSObjectReference _jsObject = null!;
    private DotNetObjectReference<AurilaApp> _dotNetRef = null!;
    private bool _isInitialized = false;
    private readonly StateFlow<string> _currentRoute = new("/");
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public IStateFlow<string> CurrentRoute => _currentRoute;

    protected override async Task OnInitializedAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        _jsObject = await jsInterop.CreateObjectAsync("AurilaApp", _dotNetRef);

        //read the initial location
        var initialLocation = await _jsObject.InvokeAsync<string>("getCurrentLocation");
        _currentRoute.Value = initialLocation;

        _isInitialized = true;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (_isInitialized)
        {
            builder.OpenComponent<CascadingValue<AurilaApp>>(0);
            {
                builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
                builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
                builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), ChildContent);
            }
            builder.CloseComponent();
        }
    }

    #region IAurilaContext

    private async ValueTask UpdateLayerCountAsync()
    {
        var layerCount = _navigationInterceptors.Count(i => i.Enabled);
        await _jsObject.InvokeVoidAsync("updateLayerCount", layerCount);
    }

    async ValueTask IAurilaContext.ToggleInterceptorAsync(INavigationInterceptor interceptor, bool enabled)
    {
        var interceptorHandle = _navigationInterceptors.FindLast(i => i.Interceptor == interceptor && i.Enabled != enabled);

        if(interceptorHandle != null)
        {
            interceptorHandle.Enabled = enabled;
            await UpdateLayerCountAsync();
        }
    }

    async ValueTask IAurilaContext.RegisterInterceptorAsync(INavigationInterceptor interceptor, bool enabled)
    {
        bool exists = _navigationInterceptors.Any(i => i.Interceptor == interceptor);

        if (exists) return;

        _navigationInterceptors.Add(new NavigationInterceptorHandle
        {
            Interceptor = interceptor,
            Enabled = enabled
        });

        await UpdateLayerCountAsync();
    }

    async ValueTask IAurilaContext.UnregisterReceiverAsync(INavigationInterceptor interceptor)
    {
        var index = _navigationInterceptors.FindLastIndex(i => i.Interceptor == interceptor);

        if (index != -1)
        {
            _navigationInterceptors.RemoveAt(index);
            await UpdateLayerCountAsync();
        }
    }

    async ValueTask IAurilaNavigationContext.CompleteNavigationAsync(RouteInfo routeInfo, NavigationType navigationType)
    {
        if(!routeInfo.Url.StartsWith('/'))
        {
            throw new ArgumentException("Expected a relative path starting with '/'", nameof(routeInfo));
        }

        await _jsObject.InvokeVoidAsync("completeNavigation", routeInfo, navigationType);
        _currentRoute.Value = routeInfo.Url;
    }

    async ValueTask<IReadOnlyList<NavEntry>> IAurilaNavigationContext.GetNavStackAsync()
    {
        return await _jsObject.InvokeAsync<IReadOnlyList<NavEntry>>("getNavStack");
    }

    [JSInvokable]
    public async Task<InterceptionResult> HandlePopStateAsync()
    {
        var interceptor = _navigationInterceptors.FindLast(i => i.Enabled)?.Interceptor;

        if (interceptor == null)
        {
            return InterceptionResult.NotHandled;
        }

        return await interceptor.HandleAsync();
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        await _jsObject.InvokeVoidAsync("dispose");
        await _jsObject.DisposeAsync();
        _dotNetRef.Dispose();
        _currentRoute.Dispose();
    }

  
}
