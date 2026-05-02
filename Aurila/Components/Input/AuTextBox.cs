using Aurila.Enums.Input;

namespace Aurila.Components.Input;

public class AuTextBox : AuTextBoxBase<AuTextBox, string>
{
    [Parameter]
    public TextBoxKeyboard Keyboard { get; set; }

    protected override string GetInputType() => Keyboard.ToString().PascalToKebabCase();

    protected override string? GetValue(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }
}
