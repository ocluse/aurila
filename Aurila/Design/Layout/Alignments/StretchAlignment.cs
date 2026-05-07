using Aurila.Contracts.Layout;
using Aurila.Components.Layout;
using Aurila.Enums.Layout;
using Microsoft.AspNetCore.Components;

namespace Aurila.Design.Layout.Alignments;

internal sealed class StretchAlignment : IBidirectionalAlignment
{
    public void BuildClass(LayoutScope scope, Axis? axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(LayoutScope scope, Axis? axis, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuGrid)
            {
                if (axis == Axis.Horizontal) builder.Add("justify-items", "stretch");
                else if (axis == Axis.Vertical) builder.Add("align-items", "stretch");
                else builder.Add("place-items", "stretch");
            }
            else if (component is IColumn or IRow)
            {
                builder.Add("align-items", "stretch");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuGrid)
            {
                if (axis == Axis.Horizontal) builder.Add("justify-self", "stretch");
                else if (axis == Axis.Vertical) builder.Add("align-self", "stretch");
                else builder.Add("place-self", "stretch");
            }
            else if (parent is IColumn or IRow)
            {
                builder.Add("align-self", "stretch");
            }
        }
    }
}
