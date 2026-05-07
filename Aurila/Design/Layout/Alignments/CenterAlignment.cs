using Aurila.Components.Layout;
using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Alignments;

internal sealed class CenterAlignment : IBidirectionalAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuBox)
            {
                builder.Add("au-box-align-center");
            }

        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuBox)
            {
                builder.Add("au-box-item-center");
            }
        }
    }

    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuGrid)
            {
                builder.Add("place-items", "center");
            }
            else if (component is IColumn or IRow)
            {
                builder.Add("align-items", "center");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuGrid)
            {
                builder.Add("place-self", "center");
            }
            else if (parent is IColumn or IRow)
            {
                builder.Add("align-self", "center");
            }
        }
    }
}
