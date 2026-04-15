using Aurila.Contracts.Navigation;

namespace Aurila.Components.Navigation;

public sealed class NavHost(
    IAurilaHost host,
    IBackInterceptor backInterceptor,
    IRouteRegistry routeRegistry)
    : ControlBase<NavHost>, IDisposable, INavigator, IBackReceiver
{
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
    public bool AutoConsumeIntent { get; set; } = true;

    private PageEntry? CurrentPage => _pages.Count > 0 ? _pages[^1] : null;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await backInterceptor.RegisterBackReceiverAsync(this);

        host.IntentReceived += OnIntentReceived;

        Type? actualStartPage = StartPage;
        object? actualStartData = StartData;

        var currentLocation = await backInterceptor.GetCurrentLocationAsync();

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

        if (AutoConsumeIntent && host != null)
        {
            var launchIntent = host.GetLaunchIntent();

            if (launchIntent is INavigateToPageIntent intent)
            {
                actualStartPage = intent.Page;
                actualStartData = intent.Data;
            }
        }

        if (actualStartPage != null)
        {
            if (typeof(IPage).IsAssignableFrom(actualStartPage) == false)
            {
                throw new InvalidOperationException("The provided initial page does not implement IPage.");
            }

            Navigate(actualStartPage, actualStartData);
        }
        else
        {
            throw new InvalidOperationException("InitialPage must be set.");
        }
    }

    private void OnIntentReceived(object? intent)
    {
        if (intent is INavigateToPageIntent navigateIntent)
        {
            if (navigateIntent.Replace)
            {
                Replace(navigateIntent.Page, navigateIntent.Data);
            }
            else
            {
                Navigate(navigateIntent.Page, navigateIntent.Data);
            }
        }
        else if (intent is INavigateBackIntent)
        {
            GoBack();
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

    public void Navigate<TPage>(object? data = null) where TPage : IPage
    {
        Navigate(typeof(TPage), data);
    }

    public void Navigate(Type pageType, object? data = null)
    {
        _ = NavigateAsync(pageType, data);
    }

    public void Replace<TPage>(object? data = null) where TPage : IPage
    {
        _ = ReplaceAsync(typeof(TPage), data);
    }

    public void Replace(Type pageType, object? data = null)
    {
        _ = ReplaceAsync(pageType, data);
    }

    public void GoBack()
    {
        _ = GoBackAsync();
    }

    private async Task NavigateAsync(Type pageType, object? data = null)
    {
        PageEntry toPage = PageEntry.Create(pageType, data);
        PageEntry? fromPage = CurrentPage;

        await PerformNavigation(NavigationType.Push, fromPage, toPage);
    }

    private async Task ReplaceAsync(Type pageType, object? data = null)
    {
        PageEntry toPage = PageEntry.Create(pageType, data);
        PageEntry? fromPage = CurrentPage;

        if (fromPage != null)
        {
            await PerformNavigation(NavigationType.Replace, fromPage, toPage);
        }
        else
        {
            await PerformNavigation(NavigationType.Push, null, toPage);
        }
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
                        host?.RequestExit();
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
        if (_isNavigating)
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

        //add the to page to the stack if this is a first time navigation:
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
                fromNotifyRouteChanged.RouteInfoChanged -= OnRouteInfoChanged;
            }

            fromPage.NavigationType = type;
            fromPage.State = PageState.NavigatingFrom;
        }
        await WaitForRenderAsync();

        

        //notify the page we are heading to it:
        toPage.EnsuredInstance.OnNavigatingTo(navigationToArgs);

        //delay for the animation to finish:
        const int delay = 300;
        await Task.Delay(delay);

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

        if (toPage.Instance is IRoutablePage toRoutablePage)
        {
            var routeInfo = toRoutablePage.GetRouteInfo();
            await UpdateRouteInfo(routeInfo);
        }

        if (toPage.Instance is INotifyRouteChanged toNotifyRouteChanged)
        {
            toNotifyRouteChanged.RouteInfoChanged += OnRouteInfoChanged;
        }

        if (navigationToArgs.DataConsumed)
        {
            toPage.Data = null;
        }

        if (_navigationQueue.TryDequeue(out var nextNavigation))
        {
            _ = nextNavigation();
        }
    }

    private async void OnRouteInfoChanged(object? sender, RouteInfo e)
    {
        await UpdateRouteInfo(e);
    }

    private async Task UpdateRouteInfo(RouteInfo routeInfo)
    {
        await backInterceptor.SetWindowLocationAsync(routeInfo.Route);
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
        builder.OpenComponent<CascadingValue<NavHost>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), (RenderFragment)(builder2 =>
            {
                builder2.OpenRegion(4);
                Render(builder2);
                builder2.CloseRegion();
            }));
        }
        builder.CloseComponent();
    }

    private void Render(RenderTreeBuilder builder)
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

    void IDisposable.Dispose()
    {
        if(CurrentPage?.Instance is INotifyRouteChanged notifyRouteChanged)
        {
            notifyRouteChanged.RouteInfoChanged -= OnRouteInfoChanged;
        }

        backInterceptor.UnregisterBackReceiver(this);

        host?.IntentReceived -= OnIntentReceived;
    }

    public bool HandleBackPressed()
    {
        if (_pages.Count == 0)
        {
            return false;
        }
        else if (_pages.Count == 1 && _pages[0].PageType == StartPage)
        {
            //only the default initial page should make us handle it as usual
            return false;
        }
        else
        {
            GoBack();
            return true;
        }
    }
}

