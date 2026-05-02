namespace Aurila.Design.Shapes;

internal sealed class RoundedShape(
    CssLength? topLeft,
    CssLength? topRight,
    CssLength? bottomRight,
    CssLength? bottomLeft) : IRoundedShape
{
    public CssLength? TopLeft => topLeft;

    public CssLength? TopRight => topRight;

    public CssLength? BottomRight => bottomRight;

    public CssLength? BottomLeft => bottomLeft;

    public void BuildClass(ComponentBase component, ClassBuilder builder) { }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.AddIf(topLeft.HasValue, "border-top-left-radius", topLeft!.Value.ToString());
        builder.AddIf(topRight.HasValue, "border-top-right-radius", topRight!.Value.ToString());
        builder.AddIf(bottomRight.HasValue, "border-bottom-right-radius", bottomRight!.Value.ToString());
        builder.AddIf(bottomLeft.HasValue, "border-bottom-left-radius", bottomLeft!.Value.ToString());
    }
}