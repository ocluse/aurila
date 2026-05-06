using Aurila.Components;
using Aurila.Design;

namespace Aurila.Contracts.Design.Appearance;

public interface IBuildingAppearance<T> : IAppearance<T>
    where T : AuControlBase<T>
{
    void BuildClass(T control, ClassBuilder builder);
    void BuildStyle(T control, StyleBuilder builder);
}
