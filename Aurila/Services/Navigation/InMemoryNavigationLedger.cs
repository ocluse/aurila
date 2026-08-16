using Aurila.Contracts.Navigation;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using System.Text.Json;

namespace Aurila.Services.Navigation;

/// <summary>
/// A browser-free <see cref="INavigationLedger"/> that models the Navigation API's entry list.
/// </summary>
/// <remarks>
/// Navigation is the part of a UI framework most likely to break in ways that only show up on a real
/// back button, and it is exactly the part that has historically been impossible to test here. This
/// implementation exists so that routing, page reuse, parameter binding and guard behaviour can be
/// exercised in unit tests. It reproduces the platform's semantics that Aurila depends on:
/// <list type="bullet">
/// <item>keys are stable across replacement,</item>
/// <item>a push truncates everything ahead of the current entry,</item>
/// <item>a replace preserves the slot's key but issues a new id,</item>
/// <item>state persists on the entry and is replayed on traversal.</item>
/// </list>
/// </remarks>
public sealed class InMemoryNavigationLedger : INavigationLedger
{
    private readonly List<MutableEntry> _entries = [];
    private readonly string _origin;
    private int _currentIndex = -1;
    private int _nextId;
    private int _nextKey;

    public InMemoryNavigationLedger(string initialPath = "/", string origin = "https://localhost")
    {
        _origin = origin.TrimEnd('/');
        _entries.Add(NewEntry(initialPath, null));
        _currentIndex = 0;
        Publish();
    }

    /// <summary>Every navigation the ledger was asked to perform, in order. For assertions.</summary>
    public List<NavigationRecord> Log { get; } = [];

    /// <summary>Whether <see cref="ActivateAsync"/> has been called.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Whether the driver has declared that a page might refuse to be left.</summary>
    public bool IsGuardArmed { get; private set; }

    public INavigationDriver? Driver { get; set; }

    public NavSnapshot Snapshot { get; private set; } = NavSnapshot.Empty;

    public event EventHandler<NavSnapshot>? SnapshotChanged;

    public bool CanGoBack => _currentIndex > 0;

    public bool CanGoForward => _currentIndex >= 0 && _currentIndex < _entries.Count - 1;

    public ValueTask<NavSnapshot> RefreshAsync() => ValueTask.FromResult(Snapshot);

    public ValueTask<NavResult> NavigateAsync(string url, NavigateOptions? options = null)
    {
        options ??= NavigateOptions.Push;
        Log.Add(new NavigationRecord(options.History == NavHistory.Replace ? "replace" : "push", url, options.Info));

        if (options.History == NavHistory.Replace && _currentIndex >= 0)
        {
            var slot = _entries[_currentIndex];
            slot.Path = url;
            slot.Id = NextId();
            slot.State = options.State is null ? slot.State : Serialize(options.State);
        }
        else
        {
            if (_currentIndex < _entries.Count - 1)
            {
                _entries.RemoveRange(_currentIndex + 1, _entries.Count - _currentIndex - 1);
            }

            _entries.Add(NewEntry(url, options.State));
            _currentIndex = _entries.Count - 1;
        }

        Publish();
        return ValueTask.FromResult(NavResult.Success);
    }

    public ValueTask<NavResult> TraverseToAsync(string entryKey, object? info = null)
    {
        Log.Add(new NavigationRecord("traverse", entryKey, info));

        var index = _entries.FindIndex(e => e.Key == entryKey);

        if (index < 0)
        {
            return ValueTask.FromResult(new NavResult(false, "InvalidStateError", $"No entry with key '{entryKey}'."));
        }

        _currentIndex = index;
        Publish();
        return ValueTask.FromResult(NavResult.Success);
    }

    public ValueTask<NavResult> BackAsync(object? info = null)
    {
        Log.Add(new NavigationRecord("back", null, info));

        if (!CanGoBack)
        {
            return ValueTask.FromResult(new NavResult(false, "InvalidStateError", "No entry to go back to."));
        }

        _currentIndex--;
        Publish();
        return ValueTask.FromResult(NavResult.Success);
    }

    public ValueTask<NavResult> ForwardAsync(object? info = null)
    {
        Log.Add(new NavigationRecord("forward", null, info));

        if (!CanGoForward)
        {
            return ValueTask.FromResult(new NavResult(false, "InvalidStateError", "No entry to go forward to."));
        }

        _currentIndex++;
        Publish();
        return ValueTask.FromResult(NavResult.Success);
    }

    public ValueTask UpdateStateAsync(object? state)
    {
        if (_currentIndex >= 0)
        {
            _entries[_currentIndex].State = Serialize(state);
            Publish();
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ActivateAsync()
    {
        IsActive = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetGuardArmedAsync(bool armed)
    {
        IsGuardArmed = armed;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private MutableEntry NewEntry(string path, object? state) => new()
    {
        Key = $"key-{_nextKey++}",
        Id = NextId(),
        Path = path,
        State = Serialize(state)
    };

    private string NextId() => $"id-{_nextId++}";

    private static JsonElement? Serialize(object? state)
        => state is null ? null : JsonSerializer.SerializeToElement(state);

    private void Publish()
    {
        Snapshot = new NavSnapshot(
            [.. _entries.Select((e, i) => new NavEntryRef(e.Key, e.Id, i, _origin + e.Path, e.Path, e.State))],
            _currentIndex);

        SnapshotChanged?.Invoke(this, Snapshot);
    }

    private sealed class MutableEntry
    {
        public required string Key { get; init; }
        public required string Id { get; set; }
        public required string Path { get; set; }
        public JsonElement? State { get; set; }
    }

    /// <summary>A navigation the ledger was asked to perform.</summary>
    public sealed record NavigationRecord(string Kind, string? Target, object? Info);
}
