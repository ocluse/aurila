namespace Aurila.Models;

public record PaddingValues
{
    public CssLength? Top { get; set; }

    public CssLength? Bottom { get; set; }

    public CssLength? Left { get; set; }

    public CssLength? Right { get; set; }

    public static PaddingValues All(CssLength? value) => new()
    {
        Top = value,
        Bottom = value,
        Left = value,
        Right = value
    };

    public static PaddingValues Symmetric(CssLength? vertical, CssLength? horizontal) => new()
    {
        Top = vertical,
        Bottom = vertical,
        Left = horizontal,
        Right = horizontal
    };

    public static implicit operator PaddingValues(CssLength value) => All(value);
}
