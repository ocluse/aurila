using Aurila.Contracts.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Components.Navigation;

public abstract class AuPage : ComponentBase, IPage, IDisposable
{
    private readonly CancellationTokenSource _disposalCts = new();
    private CancellationTokenSource _navigationCts = new(TimeSpan.Zero);

    protected CancellationToken PageDisposed => _disposalCts.Token;

    protected CancellationToken PageNavigatedFrom => _navigationCts.Token;

    [CascadingParameter]
    public INavigator Nav { get; set; } = null!;

    [CascadingParameter]
    public IAurilaContext AurilaContext { get; set; } = null!;

    void IPage.OnNavigatingTo(NavigationToArgs args)
    {
        RenewNavigationCts();
        OnNavigatingTo(args);
    }

    void IPage.OnNavigatedTo(NavigationToArgs args)
        => OnNavigatedTo(args);

    async Task IPage.OnNavigatingFromAsync(NavigationFromArgs args)
    {
        await OnNavigatingFromAsync(args);
        if (!args.Cancelled)
        {
            try
            {
                await _navigationCts.CancelAsync();
            }
            catch (ObjectDisposedException)
            {
                //do nothing
            }
        }
    }

    void IPage.OnNavigatedFrom(NavigationFromArgs args)
        => OnNavigatedFrom(args);

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