namespace Aurila.Components.Controls;

public class TimePicker : TextBoxBase<TimePicker, TimeOnly?>
{
    protected override string GetInputType() => "time";

    protected override TimeOnly? GetValue(object? value)
    {
        string? val = value?.ToString();

        return string.IsNullOrEmpty(val) ? null : TimeOnly.Parse(val);
    }

    protected override object? GetInputDisplayValue(TimeOnly? value)
    {
        return value?.ToString("HH:mm");
    }
}
