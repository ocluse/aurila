using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;
public class NavigationArgs
{
    public required object? Data { get; init; }

    public required NavigationType Type { get; init; }
}

public class NavigationFromArgs : NavigationArgs
{
    private volatile bool _cancelled;

    public bool Cancelled => _cancelled;

    public required Type? Destination { get; init; }

    public void Cancel()
    {
        _cancelled = true;
    }
}

public class NavigationToArgs : NavigationArgs
{
    private bool _dataConsumed;

    public bool DataConsumed => _dataConsumed;

    public object? ConsumeData()
    {
        if (_dataConsumed)
        {
            throw new InvalidOperationException("Data has already been consumed.");
        }

        _dataConsumed = true;

        return Data;
    }
}

