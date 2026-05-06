namespace Aurila.Components.Input;

public class AuDateTimePicker : AuTextBoxBase<AuDateTimePicker, DateTimeOffset?>
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