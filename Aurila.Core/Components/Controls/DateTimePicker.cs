using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurila.Components.Controls;

public class DateTimePicker : TextBoxBase<DateTimePicker, DateTimeOffset?>
{
    protected override string GetInputType() => "datetime-local";

    protected override DateTimeOffset? GetValue(object? value)
    {
        string? val = value?.ToString();

        return string.IsNullOrEmpty(val) ? null : DateTimeOffset.Parse(val);
    }

    protected override object? GetInputDisplayValue(DateTimeOffset? value)
    {
        return value?.ToString("yyyy-MM-ddTHH:mm");
    }
}