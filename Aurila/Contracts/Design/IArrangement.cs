using Aurila.Components;

namespace Aurila.Contracts.Design;

public interface IArrangement
{
    void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder);

    void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder);
}