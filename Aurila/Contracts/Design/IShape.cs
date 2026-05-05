namespace Aurila.Contracts.Design;

public interface IShape : IStyler
{
}

public interface IRoundedShape : IShape
{
    CssLength? TopLeft { get; }
    
    CssLength? TopRight { get; }
    
    CssLength? BottomRight { get; }
    
    CssLength? BottomLeft { get; }
}