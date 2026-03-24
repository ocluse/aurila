using Aurila.Components;
using Aurila.Contracts.Design;
using Aurila.Models;

namespace Aurila.Design.Layout;

internal class SpacedByArrangement(CssLength spacing) : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
        var property = axis is Axis.Horizontal ? "column-gap" : "row-gap";
        builder.Add(property, spacing.ToString());
    }
}
