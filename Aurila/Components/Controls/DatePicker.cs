namespace Aurila.Components.Controls;

public class DatePicker : TextBoxBase<DatePicker, DateOnly?>
{
    protected override string GetInputType() => "date";

    protected override DateOnly? GetValue(object? value)
    {
        string? val = value?.ToString();

        return string.IsNullOrEmpty(val) ? null : DateOnly.Parse(val);
    }

    protected override object? GetInputDisplayValue(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd");
    }
}