using Aurila.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout;

internal class SimpleLayoutArrangement(string className) : IArrangement
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        builder.Add(className);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
    }
}
