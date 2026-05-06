using Aurila.Components;
using Aurila.Enums.Input;

namespace Aurila.Contracts.Design.Appearance;

public interface IAppearanceProvider
{
    IIconPainter? IconPainter { get; }
    FieldHeaderStyle HeaderStyle { get; }
    IAppearance<TControl>? GetAppearance<TControl>() where TControl : AuControlBase<TControl>;
}