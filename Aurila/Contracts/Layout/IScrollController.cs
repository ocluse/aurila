namespace Aurila.Contracts.Layout;
public interface IScrollController
{
    event EventHandler<ScrollChangedEventArgs> ScrollChanged;

    Task ScrollToPositionAsync(int positionPx);

    Task ScrollToPositionAsync(double progress);

    Task ScrollToEndAsync();

    Task ScrollToStartAsync();
}
