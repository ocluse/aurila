namespace Aurila.Contracts;

public interface IHasPadding
{
    CssLength? Padding { get; }
    CssLength? PaddingHorizontal { get; }
    CssLength? PaddingVertical { get; }
    CssLength? PaddingTop { get; }
    CssLength? PaddingBottom { get; }
    CssLength? PaddingRight { get; }
    CssLength? PaddingLeft { get; }
}

public interface IHasMargin
{
    CssLength? Margin { get; }
    CssLength? MarginHorizontal { get; }
    CssLength? MarginVertical { get; }
    CssLength? MarginRight { get; }
    CssLength? MarginLeft { get; }
    CssLength? MarginTop { get; }
    CssLength? MarginBottom { get; }
}

public interface IHasBackgroundColor
{
    string? BackgroundColor { get; }
}

public interface IHasForegroundColor
{
    string? Color { get; }
}

public interface IHasShape
{
    IShape? Shape { get; }
}