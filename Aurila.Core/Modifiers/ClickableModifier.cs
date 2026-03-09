using Aurila.Contracts.Modifiers;

namespace Aurila.Modifiers;

internal class ClickableModifier : IAttributeModifier
{
    private readonly Func<Task>? _executeAsync;
    private readonly Action? _execute;
    private readonly EventCallback _callback;

    public ClickableModifier(Func<Task> executeAsync)
    {
        _executeAsync = executeAsync;
    }

    public ClickableModifier(Action execute)
    {
        _execute = execute;
    }

    public ClickableModifier(EventCallback callback)
    {
        _callback = callback;
    }

    public void BuildAttributes(ComponentBase component, IDictionary<string, object> attributes)
    {
        attributes.Add("onclick", EventCallback.Factory.Create(component, OnClickAsync));
    }

    private async Task OnClickAsync()
    {
        if (_executeAsync != null)
        {
            await _executeAsync();
        }
        else if (_execute != null)
        {
            _execute();
        }
        else if (_callback.HasDelegate)
        {
            await _callback.InvokeAsync(null);
        }
    }
}
