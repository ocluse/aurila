using Aurila.Components;

namespace Aurila.Appearance;
public abstract class DynamicAppearanceBase<T> : IBuildingAppearance<T>
    where T : ControlBase<T>
{
    public abstract void BuildClass(T control, ClassBuilder builder);
    public abstract void BuildStyle(T control, StyleBuilder builder);
}
