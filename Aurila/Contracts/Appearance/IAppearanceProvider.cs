using Aurila.Components;
using Aurila.Enums.Input;

namespace Aurila.Contracts.Appearance;

public interface IAppearanceProvider
{
    IIconPainter? IconPainter { get; }
    FieldHeaderStyle HeaderStyle { get; }
    IAppearance<TControl>? GetAppearance<TControl>() where TControl : ControlBase<TControl>;
}