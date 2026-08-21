using Aurila.Design.TextStyles;

namespace Aurila.Material.Tokens;

/// <summary>
/// The Material 3 type scale. Each entry carries a class the stylesheet defines and a sensible
/// element, so <c>&lt;AuText TextStyle="MaterialTypeScale.HeadlineLarge"&gt;</c> is also correct markup.
/// </summary>
/// <remarks>Use <c>with</c> to change the element: <c>MaterialTypeScale.TitleLarge with { ElementName = "h2" }</c>.</remarks>
public static class MaterialTypeScale
{
    public static TextStyle DisplayLarge { get; } = Make("display-large", "h1");
    public static TextStyle DisplayMedium { get; } = Make("display-medium", "h1");
    public static TextStyle DisplaySmall { get; } = Make("display-small", "h1");

    public static TextStyle HeadlineLarge { get; } = Make("headline-large", "h2");
    public static TextStyle HeadlineMedium { get; } = Make("headline-medium", "h2");
    public static TextStyle HeadlineSmall { get; } = Make("headline-small", "h2");

    public static TextStyle TitleLarge { get; } = Make("title-large", "h3");
    public static TextStyle TitleMedium { get; } = Make("title-medium", "h3");
    public static TextStyle TitleSmall { get; } = Make("title-small", "h3");

    public static TextStyle BodyLarge { get; } = Make("body-large", "p");
    public static TextStyle BodyMedium { get; } = Make("body-medium", "p");
    public static TextStyle BodySmall { get; } = Make("body-small", "p");

    public static TextStyle LabelLarge { get; } = Make("label-large", "span");
    public static TextStyle LabelMedium { get; } = Make("label-medium", "span");
    public static TextStyle LabelSmall { get; } = Make("label-small", "span");

    private static TextStyle Make(string token, string element)
        => new() { Class = $"md-typescale-{token}", ElementName = element };
}
