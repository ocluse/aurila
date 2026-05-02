using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Arrangements;

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
