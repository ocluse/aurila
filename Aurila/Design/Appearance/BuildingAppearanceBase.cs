using Aurila.Components;
using Aurila.Contracts.Appearance;

namespace Aurila.Design.Appearance;
public abstract class BuildingAppearanceBase<T> : IBuildingAppearance<T>
    where T : ControlBase<T>
{
    public abstract void BuildClass(T control, ClassBuilder builder);
    public abstract void BuildStyle(T control, StyleBuilder builder);
}
