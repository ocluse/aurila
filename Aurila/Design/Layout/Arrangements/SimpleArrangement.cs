using Aurila.Contracts.Layout;
using Aurila.Components.Layout;
using Aurila.Enums.Layout;
using Microsoft.AspNetCore.Components;

namespace Aurila.Design.Layout.Arrangements;

internal class SimpleArrangement(string cssValue) : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
        if (component is AuGrid)
        {
            builder.Add(axis == Axis.Horizontal ? "justify-content" : "align-content", cssValue);
        }
        else if (component is IRow)
        {
            builder.Add(axis == Axis.Horizontal ? "justify-content" : "align-content", cssValue);
        }
        else if (component is IColumn)
        {
            builder.Add(axis == Axis.Vertical ? "justify-content" : "align-content", cssValue);
        }
    }
}
