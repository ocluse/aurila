using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Services.Navigation;

/// <summary>
/// Batches query parameter writes into a single navigation.
/// </summary>
/// <remarks>
/// Several parameters changed in one event handler must produce one history operation, not one each.
/// Writes are collected and flushed on the next turn of the loop; if any of them asked to be
/// back-navigable, the batch is.
/// </remarks>
internal sealed class QueryWriter(Func<IReadOnlyDictionary<string, string?>, NavHistory, Task> commit)
    : IQueryParamWriter
{
    private readonly Dictionary<string, string?> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();

    private Task _inFlight = Task.CompletedTask;
    private NavHistory _history = NavHistory.Replace;
    private bool _flushScheduled;

    public void Write(string name, string? value, NavHistory history)
    {
        lock (_gate)
        {
            _pending[name] = value;

            if (history == NavHistory.Push)
            {
                _history = NavHistory.Push;
            }

            if (_flushScheduled)
            {
                return;
            }

            _flushScheduled = true;
        }

        _ = FlushAsync();
    }

    private async Task FlushAsync()
    {
        await Task.Yield();

        var previous = _inFlight;

        try
        {
            await previous;
        }
        catch
        {
        }

        Dictionary<string, string?> batch;
        NavHistory history;

        lock (_gate)
        {
            _flushScheduled = false;

            if (_pending.Count == 0)
            {
                return;
            }

            batch = new Dictionary<string, string?>(_pending, StringComparer.OrdinalIgnoreCase);
            history = _history;

            _pending.Clear();
            _history = NavHistory.Replace;
        }

        _inFlight = commit(batch, history);

        await _inFlight;
    }
}
