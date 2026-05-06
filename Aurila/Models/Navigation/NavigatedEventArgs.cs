namespace Aurila.Models.Navigation;

public class NavigatedEventArgs(Type currentPageType, string? currentPageRoute) : EventArgs
{
    public Type CurrentPageType { get; init; } = currentPageType;

    public string? CurrentRoute { get; init; } = currentPageRoute;
}
