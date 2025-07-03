using Aurila.Components;

namespace Aurila.Appearance;

public interface IBuildingAppearance<T> : IAppearance<T>
    where T : ControlBase<T>
{
    void BuildClass(T control, ClassBuilder builder);
    void BuildStyle(T control, StyleBuilder builder);
}
