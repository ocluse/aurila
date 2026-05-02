using Aurila.Design;

namespace Aurila.Components.Layout;

public class SnackbarHost : ControlBase<SnackbarHost>
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(4);

    private readonly Queue<SnackbarEntry> _queue = new();
    private readonly List<SnackbarEntry> _visible = [];

    [Parameter]
    public int MaxVisibleItems { get; set; } = 1;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-snackbar-host");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, GetAppliedAttributes());

        foreach (var entry in _visible)
        {
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "au-snackbar");
            builder.AddContent(4, entry.Message);
            builder.CloseElement();
        }

        builder.CloseElement();
    }

    public async Task ShowMessageAsync(string message, TimeSpan? duration = null)
    {
        var entry = new SnackbarEntry(message, duration ?? DefaultDuration);

        if (_visible.Count < MaxVisibleItems)
        {
            Show(entry);
        }
        else
        {
            _queue.Enqueue(entry);
        }

        await entry.CompletionSource.Task;
    }

    public void ShowMessage(string message, TimeSpan? duration = null)
    {
        var entry = new SnackbarEntry(message, duration ?? DefaultDuration);

        if (_visible.Count < MaxVisibleItems)
        {
            Show(entry);
        }
        else
        {
            _queue.Enqueue(entry);
        }
    }

    private void Show(SnackbarEntry entry)
    {
        _visible.Add(entry);
        StateHasChanged();

        _ = DismissAfterAsync(entry);
    }

    private async Task DismissAfterAsync(SnackbarEntry entry)
    {
        await Task.Delay(entry.Duration);
        Dismiss(entry);
    }

    private void Dismiss(SnackbarEntry entry)
    {
        _visible.Remove(entry);
        entry.CompletionSource.TrySetResult();

        if (_queue.TryDequeue(out var next))
        {
            Show(next);
        }
        else
        {
            StateHasChanged();
        }
    }

    private sealed class SnackbarEntry(string message, TimeSpan duration)
    {
        public string Message { get; } = message;
        public TimeSpan Duration { get; } = duration;
        public TaskCompletionSource CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
