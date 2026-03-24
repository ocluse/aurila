using Aurila.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout;

internal class SimpleArrangement(string justifyContent) : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
        builder.Add("justify-content", justifyContent);
    }
}
