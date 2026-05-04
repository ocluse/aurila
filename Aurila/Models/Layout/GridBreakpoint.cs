namespace Aurila.Models.Layout;

public record GridBreakpoint(int? MinWidth, int? MaxWidth, GridDefinition Definition)
{
    public bool Matches(int width)
    {
        if (width < MinWidth) return false;
        if (MaxWidth.HasValue && width > MaxWidth.Value) return false;
        return true;
    }
}
