namespace Aurila.Design.Shapes;


public static class Shape
{
    public static IShape None { get; } = new NoneShape();

    public static IRoundedShape Rounded(CssLength all)
       => new RoundedShape(all, all, all, all);

    public static IShape Rounded(CssLength? vertical = null, CssLength? horizontal = null)
        => new RoundedShape(vertical, horizontal, vertical, horizontal);

    public static IRoundedShape Rounded(
        CssLength? topLeft = null,
        CssLength? topRight = null,
        CssLength? bottomRight = null,
        CssLength? bottomLeft = null)
        => new RoundedShape(topLeft, topRight, bottomRight, bottomLeft);

    public static IShape Pill { get; } = new PillShape();

    public static IShape Circle { get; } = new CircleShape();

    public static IShape Cut(CssLength size) => new CutCornerShape(size);
}
