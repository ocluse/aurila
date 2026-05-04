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

    void IPage.OnNavigatingTo(NavigationToEventArgs e)
    {
        RenewNavigationCts();
        OnNavigatingTo(e);
    }

    void IPage.OnNavigatedTo(NavigationToEventArgs e)
        => OnNavigatedTo(e);

    async Task IPage.OnNavigatingFromAsync(NavigationFromEventArgs e)
    {
        await OnNavigatingFromAsync(e);
        if (!e.Cancelled)
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

    void IPage.OnNavigatedFrom(NavigationFromEventArgs e)
        => OnNavigatedFrom(e);

    protected virtual void OnNavigatingTo(NavigationToEventArgs e) { }

    protected virtual void OnNavigatedFrom(NavigationFromEventArgs e) { }

    protected virtual void OnNavigatedTo(NavigationToEventArgs e) { }

    protected virtual Task OnNavigatingFromAsync(NavigationFromEventArgs e)
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