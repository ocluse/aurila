using Aurila.Components;

namespace Aurila.Appearance;

public abstract class AppearanceProvider : IAppearanceProvider
{
    private readonly Dictionary<Type, object> _appearances = [];

    public abstract IIconPainter IconPainter { get; }

    public abstract FieldHeaderStyle HeaderStyle { get; }

    protected void RegisterAppearance<TControl>(IAppearance<TControl> appearance)
        where TControl : ControlBase<TControl>
    {
        _appearances[typeof(TControl)] = appearance;
    }

    public IAppearance<TControl>? GetAppearance<TControl>() where TControl : ControlBase<TControl>
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