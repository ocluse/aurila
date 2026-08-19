using Aurila.Design.Shapes;

namespace Aurila.Material.Tokens;

/// <summary>The Material 3 corner scale, as Aurila shapes.</summary>
public static class MaterialShapes
{
    public static IShape None { get; } = Shape.None;

    public static IShape ExtraSmall { get; } = Shape.Rounded("4px");

    public static IShape Small { get; } = Shape.Rounded("8px");

    public static IShape Medium { get; } = Shape.Rounded("12px");

    public static IShape Large { get; } = Shape.Rounded("16px");

    public static IShape ExtraLarge { get; } = Shape.Rounded("28px");

    public static IShape Full { get; } = Shape.Pill;

    public static IShape LargeTop { get; } = Shape.Rounded(topLeft: "16px", topRight: "16px");

    public static IShape ExtraLargeTop { get; } = Shape.Rounded(topLeft: "28px", topRight: "28px");
}

/// <summary>Elevation shadows, referenced as CSS custom properties.</summary>
public static class MaterialElevation
{
    public const string Level0 = "var(--md-sys-elevation-level0)";
    public const string Level1 = "var(--md-sys-elevation-level1)";
    public const string Level2 = "var(--md-sys-elevation-level2)";
    public const string Level3 = "var(--md-sys-elevation-level3)";
    public const string Level4 = "var(--md-sys-elevation-level4)";
    public const string Level5 = "var(--md-sys-elevation-level5)";
}
