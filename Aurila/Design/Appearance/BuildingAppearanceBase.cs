using Aurila.Components;
using Aurila.Contracts.Design.Appearance;

namespace Aurila.Design.Appearance;
public abstract class BuildingAppearanceBase<T> : IBuildingAppearance<T>
    where T : AuControlBase<T>
{
    public abstract void BuildClass(T control, ClassBuilder builder);
    public abstract void BuildStyle(T control, StyleBuilder builder);
}
