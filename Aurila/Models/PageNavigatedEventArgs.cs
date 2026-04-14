using Aurila.Contracts.Navigation;

namespace Aurila.Models;

public class PageNavigatedEventArgs : EventArgs
{
    public required Type PageType { get; init; }
    public required object? Data { get; init; }
    public required NavigationType NavigationType { get; init; }
    public required IPage? Instance { get; init; }
}