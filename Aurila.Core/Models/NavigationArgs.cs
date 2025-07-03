using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Models;
public class NavigationArgs
{
    public required object? Data { get; init; }

    public required NavigationType Type { get; init; }
}

public class NavigationFromEventArgs : NavigationArgs
{
    private bool _cancelled;

    public bool Cancelled => _cancelled;

    public required Type? Destination { get; init; }

    public void Cancel()
    {
        _cancelled = true;
    }
}

public class NavigationToEventArgs : NavigationArgs
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

