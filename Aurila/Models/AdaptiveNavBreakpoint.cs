namespace Aurila.Models;

public record AdaptiveNavBreakpoint(int MinWidth, int? MaxWidth, AdaptiveNavPresentation Presentation)
{
    public bool Matches(int width)
    {
        if (width < MinWidth)
        {
            return false;
        }

        if (MaxWidth.HasValue && width > MaxWidth.Value)
        {
            return false;
        }

        return true;
    }
}
