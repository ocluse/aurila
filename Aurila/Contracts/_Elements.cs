using Aurila.Contracts.Design;

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

public interface IHasSize
{
    CssLength? Width { get; }
    CssLength? Height { get; }
    CssLength? MinWidth { get; }
    CssLength? MaxWidth { get; }
    CssLength? MinHeight { get; }
    CssLength? MaxHeight { get; }
}

public interface IHasBorder
{
    string? Border { get; }
    string? BorderColor { get; }
    CssLength? BorderWidth { get; }
}

public interface IHasBackground
{
    string? Background { get; }
}

public interface IHasColor
{
    string? Color { get; }
}

public interface IHasShape
{
    IShape? Shape { get; }
}