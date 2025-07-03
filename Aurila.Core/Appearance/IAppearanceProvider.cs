using Aurila.Components;

namespace Aurila.Appearance;

public interface IAppearanceProvider
{
    IIconPainter IconPainter { get; }
    FieldHeaderStyle HeaderStyle { get; }
    IAppearance<TControl>? GetAppearance<TControl>() where TControl : ControlBase<TControl>;
}