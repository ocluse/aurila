using Aurila.Design;
using Aurila.Enums.Layout;

namespace Aurila.Contracts.Layout;

public interface IArrangement
{
    void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder);

    void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder);
}