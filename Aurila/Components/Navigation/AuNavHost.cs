using Aurila.Components.Navigation.Internal;
using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace Aurila.Components.Navigation;

public sealed class AuNavHost(IRouteRegistry routeRegistry, IRouteGenerator routeGenerator)
    : AuControlBase<AuNavHost>, IAsyncDisposable, INavigator, INavigationDriver
{
    public const int NavigationAnimationDuration = 300;

    private const int MaxPendingData = 32;

    private readonly PageStore _store = new(routeRegistry);
    private readonly List<PageEntry> _rendered = [];
    private readonly List<TaskCompletionSource> _pendingRenders = [];
    private readonly Dictionary<int, object?> _pendingData = [];
    private readonly List<INavigationGuard> _guards = [];
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    private QueryWriter? _queryWriter;
    private PageEntry? _current;
    private int _nextDataToken = 1;
    private long _shownCounter;
    private string? _leaveConfirmedFor;
    private bool _guardArmed;

    [Inject]
    private INavigationLedger Ledger { get; set; } = null!;

    [Inject]
    private ILogger<AuNavHost> Logger { get; set; } = null!;

    [Inject]
    private PageParametersCache Parameters { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Type? StartPage { get; set; }

    [Parameter]
    public RenderFragment<NavHostLayoutContext>? LayoutTemplate { get; set; }

    /// <summary>
    /// How many retained pages may stay alive at once.
    /// </summary>
    /// <remarks>
    /// A retained page keeps its component tree and its DOM while the user is elsewhere, which is why
    /// returning to it is instant. Without a ceiling a deep history would keep every one of them, so
    /// the least recently shown are dropped and rebuilt from their entry when needed.
    /// </remarks>
    [Parameter]
    public int MaxRetainedPages { get; set; } = 4;

    /// <summary>
    /// How long the page transition runs, in milliseconds.
    /// </summary>
    [Parameter]
    public int TransitionDuration { get; set; } = NavigationAnimationDuration;

    public event EventHandler<NavigatedEventArgs>? Navigated;

    public Type? CurrentPageType => _current?.PageType;

    public string? CurrentRoute => _current?.Path;

    private string LiveRoute => Ledger.Snapshot.Current?.Path ?? _current?.Path ?? "/";

    public bool CanGoBack => Ledger.CanGoBack;

    public bool CanGoForward => Ledger.CanGoForward;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Ledger.Driver = this;
        _queryWriter = new QueryWriter((values, history) =>
            GoAsync(QueryString.Merge(LiveRoute, values), null, history));

        await ShowCurrentEntryAsync();
        await Ledger.ActivateAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (_pendingRenders.Count == 0)
        {
            return;
        }

        var waiting = _pendingRenders.ToArray();
        _pendingRenders.Clear();

        foreach (var render in waiting)
        {
            render.TrySetResult();
        }
    }

    async Task INavigationDriver.RunAsync(NavigateObservation observation, CancellationToken cancellationToken)
    {
        await _navigationLock.WaitAsync(CancellationToken.None);

        try
        {
            await RunCoreAsync(observation, cancellationToken);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    private async Task RunCoreAsync(NavigateObservation observation, CancellationToken cancellationToken)
    {
        var snapshot = Ledger.Snapshot;
        var destination = snapshot.Current;

        if (destination is null)
        {
            return;
        }

        bool leaveConfirmed = _leaveConfirmedFor is { } confirmedFor
            && string.Equals(confirmedFor, observation.DestinationPath ?? string.Empty, StringComparison.Ordinal);

        _leaveConfirmedFor = null;

        var toPage = _store.Resolve(destination, StartPage);
        var intent = ResolveIntent(observation, snapshot, destination, toPage);
        var fromPage = _current;

        ApplyMemoryState(toPage, TakeMemoryState(observation));

        if (intent == NavIntent.Rebind)
        {
            _current = toPage;
            toPage.LastShownAt = Interlocked.Increment(ref _shownCounter);

            RefreshParameters(toPage);

            RebuildRenderSet();
            await WaitForRenderAsync();

            await UpdateGuardAsync();

            Navigated?.Invoke(this, new NavigatedEventArgs(toPage.PageType, toPage.Path));
            return;
        }

        if (_guardArmed && !leaveConfirmed)
        {
            await AskToLeaveAsync(new NavigationLeaveContext
            {
                Intent = intent,
                DestinationPath = observation.DestinationPath,
                CanBlock = false,
                UserInitiated = observation.UserInitiated
            });
        }

        _current = toPage;

        await TransitionAsync(fromPage, toPage, intent, cancellationToken);

        toPage.LastShownAt = Interlocked.Increment(ref _shownCounter);
        _store.Prune(Ledger.Snapshot, destination.Key, MaxRetainedPages);
        RebuildRenderSet();
        await WaitForRenderAsync();

        await UpdateGuardAsync();

        Navigated?.Invoke(this, new NavigatedEventArgs(toPage.PageType, toPage.Path));
    }

    async ValueTask<bool> INavigationDriver.ConfirmLeaveAsync(NavigateObservation observation)
    {
        var intent = observation.ResolveIntent(CurrentIndex(Ledger.Snapshot));

        await PersistStateAsync();

        var context = new NavigationLeaveContext
        {
            Intent = intent,
            DestinationPath = observation.DestinationPath,
            CanBlock = observation.Cancelable,
            UserInitiated = observation.UserInitiated
        };

        bool allowed = await AskToLeaveAsync(context);

        _leaveConfirmedFor = allowed ? observation.DestinationPath ?? string.Empty : null;

        return allowed;
    }

    private async Task<bool> AskToLeaveAsync(NavigationLeaveContext context)
    {
        foreach (var guard in _guards.ToArray())
        {
            if (!guard.IsArmed)
            {
                continue;
            }

            bool permitted;

            try
            {
                permitted = await guard.CanLeaveAsync(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "A navigation guard threw and was treated as permitting the navigation.");
                continue;
            }

            if (!permitted && context.CanBlock)
            {
                return false;
            }
        }

        return true;
    }

    private async Task ShowCurrentEntryAsync()
    {
        var snapshot = await Ledger.RefreshAsync();
        var destination = snapshot.Current;

        if (destination is null)
        {
            throw new InvalidOperationException(
                "The browser reported no current history entry, so there is nothing to show.");
        }

        var page = _store.Resolve(destination, StartPage);

        page.LastShownAt = Interlocked.Increment(ref _shownCounter);
        page.State = PageState.NavigatedTo;
        page.Intent = null;

        _current = page;

        PrepareBinding(page);

        RebuildRenderSet();
        await WaitForRenderAsync();

        await UpdateGuardAsync();

        Navigated?.Invoke(this, new NavigatedEventArgs(page.PageType, page.Path));
    }

    private async Task TransitionAsync(
        PageEntry? fromPage,
        PageEntry toPage,
        NavIntent intent,
        CancellationToken cancellationToken)
    {
        bool swapping = fromPage is not null && fromPage != toPage;
        bool resuming = toPage.Instance is not null;

        toPage.State = PageState.NavigatingTo;
        toPage.Intent = intent;

        if (swapping)
        {
            fromPage!.State = PageState.NavigatingFrom;
            fromPage.Intent = intent;
        }

        PrepareBinding(toPage);

        RebuildRenderSet(fromPage);
        await WaitForRenderAsync();

        if (resuming && toPage.Instance is AuPage resumed)
        {
            resumed.Resume();
        }

        try
        {
            await Task.Delay(Math.Max(TransitionDuration, 0), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }

        toPage.State = PageState.NavigatedTo;
        toPage.Intent = null;

        if (swapping)
        {
            fromPage!.State = PageState.NavigatedFrom;
            fromPage.Intent = null;

            if (fromPage.IsRetained && fromPage.Instance is AuPage suspended)
            {
                suspended.Suspend();
            }
        }
    }

    private NavIntent ResolveIntent(
        NavigateObservation observation,
        NavSnapshot snapshot,
        NavEntryRef destination,
        PageEntry toPage)
    {
        var intent = observation.ResolveIntent(CurrentIndex(snapshot));

        if (_current is null)
        {
            return intent;
        }

        bool sameEntry = string.Equals(_current.EntryKey, destination.Key, StringComparison.Ordinal);

        return sameEntry && toPage.PageType == _current.PageType ? NavIntent.Rebind : intent;
    }

    private int CurrentIndex(NavSnapshot snapshot)
    {
        if (_current is null)
        {
            return snapshot.CurrentIndex;
        }

        for (int i = 0; i < snapshot.Entries.Count; i++)
        {
            if (string.Equals(snapshot.Entries[i].Key, _current.EntryKey, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return snapshot.CurrentIndex;
    }

    private void RebuildRenderSet(PageEntry? outgoing = null)
    {
        _rendered.Clear();

        foreach (var page in _store.Live.OrderBy(p => p.EntryKey, StringComparer.Ordinal))
        {
            if (page.IsRetained && page.Instance is not null && page != _current && page != outgoing)
            {
                _rendered.Add(page);
            }
        }

        if (outgoing is not null && outgoing != _current)
        {
            _rendered.Add(outgoing);
        }

        if (_current is not null)
        {
            _rendered.Add(_current);
        }
    }

    private void PrepareBinding(PageEntry page)
    {
        page.Binding.RouteParameters = page.RouteParameters;
        page.Binding.Writer = _queryWriter;
    }

    private void RefreshParameters(PageEntry page)
    {
        PrepareBinding(page);

        if (page.Instance is not { } instance)
        {
            return;
        }

        Parameters.RefreshHolders(instance, page.RouteParameters);

        if (instance is AuPage renderable)
        {
            renderable.NotifyStateChanged();
        }
    }

    private async Task UpdateGuardAsync()
    {
        bool armed = _guards.Any(g => g.IsArmed);

        if (armed == _guardArmed)
        {
            return;
        }

        await Ledger.SetGuardArmedAsync(armed);

        _guardArmed = armed;
    }

    private async Task WaitForRenderAsync()
    {
        var rendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _pendingRenders.Add(rendered);

        await InvokeAsync(StateHasChanged);
        await rendered.Task;
    }

    public void Navigate(NavTarget target, object? state = null)
        => Start(NavigateCoreAsync(target, state, NavHistory.Push));

    public void Navigate<TPage>(object? state = null, object? routeValues = null) where TPage : IPage
        => Start(NavigateCoreAsync(NavTarget.To<TPage>(routeValues), state, NavHistory.Push));

    public void Replace(NavTarget target, object? state = null)
        => Start(NavigateCoreAsync(target, state, NavHistory.Replace));

    public void Replace<TPage>(object? state = null, object? routeValues = null) where TPage : IPage
        => Start(NavigateCoreAsync(NavTarget.To<TPage>(routeValues), state, NavHistory.Replace));

    public string GetUrl(NavTarget target) => routeGenerator.GetUrl(target);

    public bool TryGetUrl(NavTarget target, out string url) => routeGenerator.TryGetUrl(target, out url);

    public void GoBack() => Start(PersistThen(() => Ledger.BackAsync().AsTask()));

    public void GoForward() => Start(PersistThen(() => Ledger.ForwardAsync().AsTask()));

    public void AddGuard(INavigationGuard guard)
    {
        if (!_guards.Contains(guard))
        {
            _guards.Add(guard);
            RefreshGuards();
        }
    }

    public void RemoveGuard(INavigationGuard guard)
    {
        if (_guards.Remove(guard))
        {
            RefreshGuards();
        }
    }

    public void RefreshGuards() => Start(UpdateGuardAsync());

    Task INavigationDriver.PersistStateAsync() => PersistStateAsync();

    public async Task PersistStateAsync()
    {
        if (_current is not { Instance: not null } entry)
        {
            return;
        }

        if (!string.Equals(Ledger.Snapshot.Current?.Key, entry.EntryKey, StringComparison.Ordinal))
        {
            return;
        }

        Dictionary<string, object?> durable;

        try
        {
            var (captured, memory) = Parameters.CaptureState(entry.Instance);

            durable = captured;

            foreach (var (key, value) in memory)
            {
                entry.MemoryState[key] = value;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Capturing state for {Page} failed.", entry.PageType.Name);
            return;
        }

        try
        {
            await Ledger.UpdateStateAsync(durable.Count == 0 ? null : durable);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Persisting state for {Page} failed.", entry.PageType.Name);
        }
    }

    private static void ApplyMemoryState(PageEntry page, IReadOnlyDictionary<string, object?>? state)
    {
        if (state is null)
        {
            return;
        }

        foreach (var (key, value) in state)
        {
            page.MemoryState[key] = value;
        }
    }

    private IReadOnlyDictionary<string, object?>? TakeMemoryState(NavigateObservation observation)
        => TakeData(observation) as IReadOnlyDictionary<string, object?>;

    private async Task PersistThen(Func<Task> operation)
    {
        await PersistStateAsync();
        await operation();
    }

    public void UpdateUrl(string route, NavHistory history = NavHistory.Replace)
        => Start(GoAsync(route, null, history));

    public void SetQuery(
        IReadOnlyDictionary<string, string?> parameters,
        NavHistory history = NavHistory.Replace)
        => Start(GoAsync(QueryString.Merge(LiveRoute, parameters), null, history, rebind: true));

    public void SetQuery(string name, string? value, NavHistory history = NavHistory.Replace)
        => SetQuery(new Dictionary<string, string?> { [name] = value }, history);

    private void Start(Task navigation)
    {
        _ = navigation.ContinueWith(
            t => Logger.LogError(t.Exception, "Navigation failed."),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task NavigateCoreAsync(NavTarget target, object? state, NavHistory history)
    {
        if (target.IsEmpty)
        {
            throw new InvalidOperationException("The navigation target is empty.");
        }

        if (target.PageType is { } declared && !typeof(IPage).IsAssignableFrom(declared))
        {
            throw new InvalidOperationException($"'{declared.FullName}' does not implement {nameof(IPage)}.");
        }

        string url = routeGenerator.GetUrl(target);
        var pageType = target.PageType ?? _store.PeekPageType(url, StartPage);
        var (durable, memory) = Parameters.SplitState(pageType, state);

        if (pageType is not null && typeof(ISingletonPage).IsAssignableFrom(pageType))
        {
            var existing = _store.FindEntryForPath(Ledger.Snapshot, url);

            if (existing is not null)
            {
                if (string.Equals(existing.Key, _current?.EntryKey, StringComparison.Ordinal))
                {
                    return;
                }

                await PersistStateAsync();

                var token = StashData(memory);
                var traversal = await Ledger.TraverseToAsync(existing.Key, token);

                if (!traversal.Committed && !traversal.IsAborted && token is not null)
                {
                    _pendingData.Remove(token.AuData);
                }

                return;
            }
        }

        await GoAsync(url, memory, history, durable);
    }

    private async Task GoAsync(
        string route,
        object? memoryState,
        NavHistory history,
        Dictionary<string, object?>? durableState = null,
        bool rebind = false)
    {
        await PersistStateAsync();

        var info = StashData(memoryState, rebind);

        var result = await Ledger.NavigateAsync(route, new NavigateOptions
        {
            History = history,
            Info = info,
            State = MergeWithCurrentState(durableState, history)
        });

        if (!result.Committed && !result.IsAborted && info is not null)
        {
            _pendingData.Remove(info.AuData);
        }
    }

    /// <summary>
    /// A replace keeps the entry, so state it does not mention must be carried across rather than
    /// dropped.
    /// </summary>
    private object? MergeWithCurrentState(Dictionary<string, object?>? durableState, NavHistory history)
    {
        if (durableState is not { Count: > 0 })
        {
            return null;
        }

        if (history != NavHistory.Replace
            || Ledger.Snapshot.Current?.State is not { ValueKind: System.Text.Json.JsonValueKind.Object } existing)
        {
            return durableState;
        }

        var merged = new Dictionary<string, object?>(durableState);

        foreach (var property in existing.EnumerateObject())
        {
            if (!merged.ContainsKey(property.Name))
            {
                merged[property.Name] = property.Value.Clone();
            }
        }

        return merged;
    }

    private DataToken? StashData(object? data, bool rebind = false)
    {
        bool empty = data is null or IReadOnlyDictionary<string, object?> { Count: 0 };

        if (empty && !rebind)
        {
            return null;
        }

        if (empty)
        {
            return new DataToken(0, rebind);
        }

        while (_pendingData.Count >= MaxPendingData)
        {
            _pendingData.Remove(_pendingData.Keys.Min());
        }

        int token = _nextDataToken++;
        _pendingData[token] = data;

        return new DataToken(token, rebind);
    }

    private object? TakeData(NavigateObservation observation)
    {
        if (observation.Info is not { ValueKind: System.Text.Json.JsonValueKind.Object } info
            || !info.TryGetProperty("auData", out var property)
            || !property.TryGetInt32(out int token))
        {
            return null;
        }

        if (!_pendingData.Remove(token, out var data))
        {
            return null;
        }

        return data;
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
                    var context = new NavHostLayoutContext(this, CurrentPageType, CurrentRoute, content);

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

            foreach (var entry in _rendered)
            {
                builder.OpenComponent<PageRenderer>(2);
                builder.SetKey(entry.EntryKey);
                builder.AddAttribute(3, nameof(PageRenderer.Entry), entry);
                builder.CloseComponent();
            }
        }
        builder.CloseElement();
    }

    public ValueTask DisposeAsync()
    {
        if (ReferenceEquals(Ledger.Driver, this))
        {
            Ledger.Driver = null;
        }

        _guards.Clear();
        _pendingData.Clear();

        return ValueTask.CompletedTask;
    }

    private sealed class DataToken(int token, bool rebind)
    {
        [System.Text.Json.Serialization.JsonPropertyName("auData")]
        public int AuData { get; } = token;

        /// <summary>
        /// Marks a URL change that keeps the page, so the ledger does not hold it back for a guard
        /// the page would be prompting itself with.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("auRebind")]
        public bool AuRebind { get; } = rebind;
    }
}
