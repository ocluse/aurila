using Aurila.Contracts.Layout;
using Aurila.Components.Layout;
using Aurila.Enums.Layout;
using Microsoft.AspNetCore.Components;

namespace Aurila.Design.Layout.Alignments;

internal sealed class EndAlignment : IHorizontalAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuGrid)
            {
                builder.Add("justify-items", "end");
            }
            else if (component is IColumn)
            {
                builder.Add("align-items", "flex-end");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuGrid)
            {
                builder.Add("justify-self", "end");
            }
            else if (parent is IColumn)
            {
                builder.Add("align-self", "flex-end");
            }
        }
    }
}
