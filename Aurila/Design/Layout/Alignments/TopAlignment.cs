using Aurila.Components.Layout;
using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Alignments;

internal sealed class TopAlignment : IVerticalAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuBox)
            {
                builder.Add("au-box-align-top-center");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;

            if (parent is AuBox)
            {
                builder.Add("au-box-item-top-center");
            }
        }
    }

    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is IRow)
            {
                builder.Add("align-items", "flex-start");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;

            if (parent is IRow)
            {
                builder.Add("align-self", "flex-start");
            }
        }
    }
}
