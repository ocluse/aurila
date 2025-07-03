using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;
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
