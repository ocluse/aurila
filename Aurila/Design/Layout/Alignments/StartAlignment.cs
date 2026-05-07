using Aurila.Contracts.Layout;
using Aurila.Components.Layout;
using Aurila.Enums.Layout;
using Microsoft.AspNetCore.Components;

namespace Aurila.Design.Layout.Alignments;

internal sealed class StartAlignment : IHorizontalAlignment
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
                builder.Add("justify-items", "start");
            }
            else if (component is IColumn)
            {
                builder.Add("align-items", "start");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuGrid)
            {
                builder.Add("justify-self", "start");
            }
            else if (parent is IColumn)
            {
                builder.Add("align-self", "start");
            }
        }
    }
}
