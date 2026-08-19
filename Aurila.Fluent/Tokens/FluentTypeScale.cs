using Aurila.Design.TextStyles;

namespace Aurila.Fluent.Tokens;

/// <summary>The Fluent 2 web type ramp.</summary>
public static class FluentTypeScale
{
    public static TextStyle Display { get; } = Make("display", "h1");
    public static TextStyle LargeTitle { get; } = Make("large-title", "h1");
    public static TextStyle Title1 { get; } = Make("title-1", "h2");
    public static TextStyle Title2 { get; } = Make("title-2", "h2");
    public static TextStyle Title3 { get; } = Make("title-3", "h3");
    public static TextStyle Subtitle1 { get; } = Make("subtitle-1", "h3");
    public static TextStyle Subtitle2 { get; } = Make("subtitle-2", "h4");
    public static TextStyle Subtitle2Stronger { get; } = Make("subtitle-2-stronger", "h4");
    public static TextStyle Body1 { get; } = Make("body-1", "p");
    public static TextStyle Body1Strong { get; } = Make("body-1-strong", "p");
    public static TextStyle Body1Stronger { get; } = Make("body-1-stronger", "p");
    public static TextStyle Body2 { get; } = Make("body-2", "p");
    public static TextStyle Caption1 { get; } = Make("caption-1", "span");
    public static TextStyle Caption1Strong { get; } = Make("caption-1-strong", "span");
    public static TextStyle Caption1Stronger { get; } = Make("caption-1-stronger", "span");
    public static TextStyle Caption2 { get; } = Make("caption-2", "span");
    public static TextStyle Caption2Strong { get; } = Make("caption-2-strong", "span");
    private static TextStyle Make(string token, string element) => new() { Class = $"fl-typography-{token}", ElementName = element };
}
