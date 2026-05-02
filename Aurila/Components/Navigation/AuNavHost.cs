using Aurila.Components.Navigation.Internal;
using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Components.Navigation;

public sealed class AuNavHost(
    IRouteRegistry routeRegistry)
    : AuControlBase<AuNavHost>, IAsyncDisposable, INavigator, INavigationInterceptor
{
    public const int NavigationAnimationDuration = 300;

    private readonly List<PageEntry> _pages = [];

    private TaskCompletionSource? _tcsRender;
    private bool _shouldWaitForRender;

    private bool _isNavigating;
    private readonly Queue<Func<Task>> _navigationQueue = new();

    [Parameter]
    [EditorRequired]
    public Type? StartPage { get; set; }

    [Parameter]
    public object? StartData { get; set; } = null;

    [Parameter]
    public RenderFragment<NavHostLayoutContext>? LayoutTemplate { get; set; }

    [CascadingParameter]
    IAurilaNavigationContext AurilaContext { get; set; } = null!;

    private PageEntry? CurrentPage => _pages.Count > 0 ? _pages[^1] : null;

    Type? INavigator.CurrentPageType => CurrentPage?.PageType;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Type? actualStartPage = StartPage;
        object? actualStartData = StartData;

        await AurilaContext.RegisterInterceptorAsync(this);

        var currentLocation = AurilaContext.CurrentRoute.Value;

        if (currentLocation.IsNotEmpty() && currentLocation != "/")
        {
            //try to find a page matching the current location:
            var routeMatch = routeRegistry.Match(currentLocation, null)
                ?? routeRegistry.GetFallbackRoute();

            if (routeMatch != null)
            {
                actualStartPage = routeMatch.PageType;
                actualStartData = routeMatch.Data;
            }
        }

        if (actualStartPage != null)
        {
            if (typeof(IPage).IsAssignableFrom(actualStartPage) == false)
            {
                throw new InvalidOperationException("The provided initial page does not implement IPage.");
            }

            _ = NavigateAsync(actualStartPage, actualStartData);
        }
        else
        {
            throw new InvalidOperationException("InitialPage must be set.");
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (_shouldWaitForRender && _tcsRender != null && !_tcsRender.Task.IsCompleted)
        {
            _tcsRender.SetResult();
        }
    }

    void INavigator.Navigate<TPage>(object? data)
    {
        _ = NavigateAsync(typeof(TPage), data);
    }

    void INavigator.Navigate(Type pageType, object? data)
    {
        _ = NavigateAsync(pageType, data);
    }

    void INavigator.Replace<TPage>(object? data)
    {
        _ = ReplaceAsync(typeof(TPage), data, null);
    }

    void INavigator.Replace(Type pageType, object? data)
    {
        _ = ReplaceAsync(pageType, data, null);
    }

    public void GoBack()
    {
        _ = GoBackAsync();
    }

    private async Task NavigateAsync(Type pageType, object? data)
    {
        PageEntry toPage = PageEntry.Create(pageType, data, null);
        PageEntry? fromPage = CurrentPage;

        await PerformNavigation(NavigationType.Push, fromPage, toPage);
    }

    private async Task ReplaceAsync(Type pageType, object? data, string? route)
    {
        PageEntry toPage = PageEntry.Create(pageType, data, route);
        PageEntry? fromPage = CurrentPage;
        NavigationType type = fromPage != null ? NavigationType.Replace : NavigationType.Push;
        await PerformNavigation(type, fromPage, toPage);
    }

    private async Task GoBackAsync()
    {
        if (_pages.Count < 2)
        {
            //We still need to call onNavigatingFrom to clean up the current page:
            var instance = CurrentPage?.Instance;

            if (instance != null)
            {
                var args = new NavigationFromEventArgs
                {
                    Data = null,
                    Type = NavigationType.Pop,
                    Destination = null
                };

                await instance.OnNavigatingFromAsync(args);

                if (!args.Cancelled)
                {
                    if (StartPage != null && instance.GetType() != StartPage)
                    {
                        //Navigate to the default page:
                        await NavigateAsync(StartPage, null);
                    }
                    else
                    {
                        //TODO: implement
                    }
                }
            }
        }
        else
        {
            var toPage = _pages[^2];

            var fromPage = _pages[^1];
            await PerformNavigation(NavigationType.Pop, fromPage, toPage);
        }
    }

    private async Task PerformNavigation(NavigationType type, PageEntry? fromPage, PageEntry toPage)
    {
        if (_isNavigating || _navigationQueue.Count > 0)
        {
            _navigationQueue.Enqueue(() => PerformNavigationCore(type, fromPage, toPage));
            return;
        }

        await PerformNavigationCore(type, fromPage, toPage);
    }

    private async Task PerformNavigationCore(NavigationType type, PageEntry? fromPage, PageEntry toPage)
    {
        _isNavigating = true;

        NavigationFromEventArgs navigationFromArgs = new()
        {
            Data = toPage.Data,
            Destination = toPage.PageType,
            Type = type,
        };

        if (fromPage?.Instance != null)
        {
            await fromPage.Instance.OnNavigatingFromAsync(navigationFromArgs);
        }

        if (navigationFromArgs.Cancelled)
        {
            _isNavigating = false;

            if (_navigationQueue.TryDequeue(out var cancelledNextNavigation))
            {
                _ = cancelledNextNavigation();
            }

            return;
        }

        NavigationToEventArgs navigationToArgs = new()
        {
            Data = toPage.Data,
            Type = type,
        };

        //add to stack if we're navigating forward, or replacing.
        if (type is NavigationType.Push or NavigationType.Replace)
        {
            _pages.Add(toPage);
        }

        //apply navigating states:
        toPage.State = PageState.NavigatingTo;
        toPage.NavigationType = type;
        if (fromPage != null)
        {
            if (fromPage.Instance is INotifyRouteChanged fromNotifyRouteChanged)
            {
                fromNotifyRouteChanged.RouteChanged -= OnRouteChanged;
            }

            fromPage.NavigationType = type;
            fromPage.State = PageState.NavigatingFrom;
        }

        await WaitForRenderAsync();

        //notify the page we are heading to it:
        toPage.EnsuredInstance.OnNavigatingTo(navigationToArgs);

        //delay for the animation to finish:
        await Task.Delay(NavigationAnimationDuration);

        //apply navigated states and modify stack:
        toPage.State = PageState.NavigatedTo;
        toPage.NavigationType = null;
        if (fromPage != null)
        {
            fromPage.NavigationType = null;
            fromPage.State = PageState.NavigatedFrom;

            if (type is NavigationType.Pop or NavigationType.Replace)
            {
                _pages.Remove(fromPage);
            }
        }

        await WaitForRenderAsync();

        //if the from page still exists, notify it we just left:
        if (fromPage?.Instance != null)
        {
            fromPage.Instance.OnNavigatedFrom(navigationFromArgs);
        }

        _isNavigating = false;

        //notify the page we have arrived:
        toPage.EnsuredInstance.OnNavigatedTo(navigationToArgs);

        await CompleteNavigationAsync(toPage, type);

        if (toPage.Instance is INotifyRouteChanged toNotifyRouteChanged)
        {
            toNotifyRouteChanged.RouteChanged += OnRouteChanged;
        }

        if (navigationToArgs.DataConsumed)
        {
            toPage.Data = null;
        }

        if (_navigationQueue.TryDequeue(out var nextNavigation))
        {
            _ = nextNavigation();
        }

        Navigated?.Invoke(this, new NavigatedEventArgs(toPage.PageType, AurilaContext.CurrentRoute.Value));
    }

    private async Task CompleteNavigationAsync(PageEntry page, NavigationType navigationType)
    {
        RouteInfo routeInfo;

        if (page.Instance is IRoutablePage routablePage)
        {
            routeInfo = routablePage.GetRouteInfo();
        }
        else if (page.Route != null)
        {
            routeInfo = new(page.Route, null);
        }
        else
        {
            var routeTemplate = routeRegistry.GetRouteTemplate(page.PageType);

            if (routeTemplate != null && !routeTemplate.HasTemplates)
            {
                routeInfo = new(routeTemplate.Template, null);
            }
            else
            {
                routeInfo = new(AurilaContext.CurrentRoute.Value, null);
            }
        }

        page.Route = routeInfo.Url;

        await AurilaContext.CompleteNavigationAsync(routeInfo, navigationType);
    }

    private async void OnRouteChanged(object? sender, RouteInfoChangedEventArgs e)
    {
        if (sender == CurrentPage?.Instance)
        {
            CurrentPage?.Route = e.Info.Url;
            await AurilaContext.CompleteNavigationAsync(e.Info, NavigationType.Replace);
        }
    }

    private async Task WaitForRenderAsync()
    {
        _tcsRender = new();
        _shouldWaitForRender = true;

        await InvokeAsync(StateHasChanged);

        await _tcsRender.Task;
        _shouldWaitForRender = false;
        _tcsRender = null;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<AuNavHost>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), (RenderFragment)(builder2 =>
            {
                var content = (RenderFragment)RenderContent;

                if (LayoutTemplate != null)
                {
                    var context = new NavHostLayoutContext(this, CurrentPage?.PageType, AurilaContext.CurrentRoute.Value, content);

                    builder2.OpenComponent<CascadingValue<NavHostLayoutContext>>(4);
                    {
                        builder2.AddAttribute(5, nameof(CascadingValue<>.Value), context);
                        builder2.AddAttribute(6, nameof(CascadingValue<>.IsFixed), false);
                        builder2.AddAttribute(7, nameof(CascadingValue<>.ChildContent), LayoutTemplate(context));
                    }
                    builder2.CloseComponent();
                }
                else
                {
                    builder2.OpenRegion(8);
                    content(builder2);
                    builder2.CloseRegion();
                }
            }));
        }
        builder.CloseComponent();
    }

    private void RenderContent(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddAttribute(1, "class", "nav-host");
            foreach (var entry in _pages)
            {
                builder.OpenComponent<PageRenderer>(2);
                builder.SetKey(entry.Id);
                builder.AddAttribute(3, nameof(PageRenderer.Entry), entry);
                builder.CloseComponent();
            }
        }
        builder.CloseElement();
    }

    public async Task<InterceptionResult> HandleAsync()
    {
        if (_pages.Count == 0)
        {
            return InterceptionResult.NotHandled;
        }
        else if (_pages.Count == 1 && _pages[0].PageType == StartPage)
        {
            return InterceptionResult.NotHandled;
        }
        else
        {
            await GoBackAsync();
            return InterceptionResult.Navigating;
        }
    }

    public void Navigate(string route)
    {
        var routeMatch = routeRegistry.Match(route, null)
            ?? routeRegistry.GetFallbackRoute();

        if (routeMatch != null)
        {
            var pageType = routeMatch.PageType;
            var data = routeMatch.Data;

            _ = NavigateAsync(pageType, data);
        }
    }

    public void Replace(string route)
    {
        var routeMatch = routeRegistry.Match(route, null)
            ?? routeRegistry.GetFallbackRoute();

        if (routeMatch != null)
        {
            var pageType = routeMatch.PageType;
            var data = routeMatch.Data;

            _ = ReplaceAsync(pageType, data, route);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (CurrentPage?.Instance is INotifyRouteChanged notifyRouteChanged)
        {
            notifyRouteChanged.RouteChanged -= OnRouteChanged;
        }

        await AurilaContext.UnregisterReceiverAsync(this);
    }
}

