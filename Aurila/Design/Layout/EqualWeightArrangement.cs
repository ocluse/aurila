using Aurila.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout;

internal class EqualWeightArrangement : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
        builder.Add("au-arrangement-equal-weight");
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
    }
}
