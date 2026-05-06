namespace Aurila.Models.Input;
public record FormContext(bool Enabled, Func<Task> Submit)
{
    public bool Disabled => !Enabled;
}
