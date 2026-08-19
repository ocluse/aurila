using Aurila.Components;
using Aurila.Components.Controls;

namespace Aurila.Fluent.Components;

public enum FluentToastIntent
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// Fluent 2 toast presenter with intent, title, body, dismissal, and a small queue.
/// It complements Aurila's generic snackbar host with Fluent-native notification anatomy.
/// </summary>
public sealed class AuFluentToastHost : AuControlBase<AuFluentToastHost>
{
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(5);
    private readonly Queue<ToastEntry> _queue = new();
    private readonly List<ToastEntry> _visible = [];

    [Parameter]
    public int MaxVisibleItems { get; set; } = 3;

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("fl-toaster");
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);
        attributes["role"] = "region";
        attributes["aria-label"] = "Notifications";
        attributes["aria-live"] = "polite";
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, GetAppliedAttributes());

        foreach (ToastEntry entry in _visible)
        {
            builder.OpenElement(2, "article");
            builder.SetKey(entry);
            builder.AddAttribute(3, "class", $"fl-toast fl-toast--{entry.Intent.ToString().ToLowerInvariant()}");
            builder.AddAttribute(4, "role", entry.Intent == FluentToastIntent.Error ? "alert" : "status");

            builder.OpenComponent<AuIcon>(5);
            builder.AddAttribute(6, nameof(AuIcon.Content), IconPath(entry.Intent));
            builder.AddAttribute(7, nameof(AuIcon.Size), (CssLength)20);
            builder.AddAttribute(8, nameof(AuIcon.Class), "fl-toast__icon");
            builder.CloseComponent();

            builder.OpenElement(9, "div");
            builder.AddAttribute(10, "class", "fl-toast__content");
            builder.OpenElement(11, "strong");
            builder.AddAttribute(12, "class", "fl-toast__title");
            builder.AddContent(13, entry.Title);
            builder.CloseElement();
            if (!string.IsNullOrWhiteSpace(entry.Body))
            {
                builder.OpenElement(14, "span");
                builder.AddAttribute(15, "class", "fl-toast__body");
                builder.AddContent(16, entry.Body);
                builder.CloseElement();
            }
            builder.CloseElement();

            builder.OpenElement(17, "button");
            builder.AddAttribute(18, "type", "button");
            builder.AddAttribute(19, "class", "fl-toast__dismiss");
            builder.AddAttribute(20, "aria-label", "Dismiss notification");
            builder.AddAttribute(21, "onclick", EventCallback.Factory.Create(this, () => Dismiss(entry)));
            builder.OpenComponent<AuIcon>(22);
            builder.AddAttribute(23, nameof(AuIcon.Content), "<path d=\"M7.28 6.22a.75.75 0 0 0-1.06 1.06L10.94 12l-4.72 4.72a.75.75 0 1 0 1.06 1.06L12 13.06l4.72 4.72a.75.75 0 1 0 1.06-1.06L13.06 12l4.72-4.72a.75.75 0 0 0-1.06-1.06L12 10.94 7.28 6.22Z\"/>");
            builder.AddAttribute(24, nameof(AuIcon.Size), (CssLength)16);
            builder.CloseComponent();
            builder.CloseElement();

            builder.CloseElement();
        }

        builder.CloseElement();
    }

    public void ShowToast(string title, string? body = null, FluentToastIntent intent = FluentToastIntent.Info, TimeSpan? duration = null)
    {
        var entry = new ToastEntry(title, body, intent, duration ?? DefaultDuration);
        if (_visible.Count < MaxVisibleItems) Show(entry);
        else _queue.Enqueue(entry);
    }

    public Task ShowToastAsync(string title, string? body = null, FluentToastIntent intent = FluentToastIntent.Info, TimeSpan? duration = null)
    {
        var entry = new ToastEntry(title, body, intent, duration ?? DefaultDuration);
        if (_visible.Count < MaxVisibleItems) Show(entry);
        else _queue.Enqueue(entry);
        return entry.Completion.Task;
    }

    public void ShowMessage(string message, TimeSpan? duration = null) => ShowToast(message, duration: duration);

    private void Show(ToastEntry entry)
    {
        _visible.Add(entry);
        StateHasChanged();
        _ = DismissAfterAsync(entry);
    }

    private async Task DismissAfterAsync(ToastEntry entry)
    {
        await Task.Delay(entry.Duration);
        await InvokeAsync(() => Dismiss(entry));
    }

    private void Dismiss(ToastEntry entry)
    {
        if (!_visible.Remove(entry)) return;
        entry.Completion.TrySetResult();
        if (_queue.TryDequeue(out ToastEntry? next)) Show(next);
        else StateHasChanged();
    }

    private static string IconPath(FluentToastIntent intent) => intent switch
    {
        FluentToastIntent.Success => "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm4.28 7.78-5 5a.75.75 0 0 1-1.06 0l-2.5-2.5a.75.75 0 0 1 1.06-1.06l1.97 1.97 4.47-4.47a.75.75 0 1 1 1.06 1.06Z\"/>",
        FluentToastIntent.Warning => "<path d=\"M10.3 3.7a1.96 1.96 0 0 1 3.4 0l8.02 13.92A1.96 1.96 0 0 1 20.02 20H3.98a1.96 1.96 0 0 1-1.7-2.38L10.3 3.7ZM12 8a.75.75 0 0 0-.75.75v4.5a.75.75 0 0 0 1.5 0v-4.5A.75.75 0 0 0 12 8Zm0 8.75a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z\"/>",
        FluentToastIntent.Error => "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm-3.28 6.72a.75.75 0 0 1 1.06 0L12 10.94l2.22-2.22a.75.75 0 1 1 1.06 1.06L13.06 12l2.22 2.22a.75.75 0 1 1-1.06 1.06L12 13.06l-2.22 2.22a.75.75 0 1 1-1.06-1.06L10.94 12 8.72 9.78a.75.75 0 0 1 0-1.06Z\"/>",
        _ => "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm0 8.5a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0v-5A.75.75 0 0 1 12 10.5ZM12 7a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z\"/>",
    };

    private sealed record ToastEntry(string Title, string? Body, FluentToastIntent Intent, TimeSpan Duration)
    {
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
