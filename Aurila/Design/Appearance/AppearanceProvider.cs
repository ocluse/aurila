using Aurila.Components;
using Aurila.Contracts.Design.Appearance;
using Aurila.Enums.Input;

namespace Aurila.Design.Appearance;

public abstract class AppearanceProvider : IAppearanceProvider
{
    private readonly Dictionary<Type, object> _appearances = [];

    public abstract IIconPainter IconPainter { get; }

    public abstract FieldHeaderStyle HeaderStyle { get; }

    protected void RegisterAppearance<TControl>(IAppearance<TControl> appearance)
        where TControl : AuControlBase<TControl>
    {
        _appearances[typeof(TControl)] = appearance;
    }

    public IAppearance<TControl>? GetAppearance<TControl>() where TControl : AuControlBase<TControl>
    {
        if (_appearances.TryGetValue(typeof(TControl), out var appearance))
        {
            return appearance as IAppearance<TControl>;
        }
        else
        {
            return null;
        }
    }
}