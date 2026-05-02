using Aurila.Enums.Input;

namespace Aurila.Components.Input;

public class TextBox : TextBoxBase<TextBox, string>
{
    [Parameter]
    public TextBoxKeyboard Keyboard { get; set; }

    protected override string GetInputType() => Keyboard.ToString().PascalToKebabCase();

    protected override string? GetValue(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }
}
