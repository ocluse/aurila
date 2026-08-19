using Aurila.Design.Shapes;

namespace Aurila.Fluent.Tokens;

public static class FluentShapes
{
    public static IShape None { get; } = Shape.None;
    public static IShape Small { get; } = Shape.Rounded("2px");
    public static IShape Medium { get; } = Shape.Rounded("4px");
    public static IShape Large { get; } = Shape.Rounded("6px");
    public static IShape ExtraLarge { get; } = Shape.Rounded("8px");
    public static IShape Circular { get; } = Shape.Pill;
}

public static class FluentElevation
{
    public const string Shadow2 = "var(--fluent-shadow-2)";
    public const string Shadow4 = "var(--fluent-shadow-4)";
    public const string Shadow8 = "var(--fluent-shadow-8)";
    public const string Shadow16 = "var(--fluent-shadow-16)";
    public const string Shadow28 = "var(--fluent-shadow-28)";
    public const string Shadow64 = "var(--fluent-shadow-64)";
}
