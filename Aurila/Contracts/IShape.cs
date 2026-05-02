using Aurila.Design;

namespace Aurila.Contracts;

public interface IShape
{
    void BuildClass(ComponentBase component, ClassBuilder builder);
    void BuildStyle(ComponentBase component, StyleBuilder builder);
}

public interface IRoundedShape : IShape
{
    CssLength? TopLeft { get; }
    CssLength? TopRight { get; }
    CssLength? BottomRight { get; }
    CssLength? BottomLeft { get; }
}