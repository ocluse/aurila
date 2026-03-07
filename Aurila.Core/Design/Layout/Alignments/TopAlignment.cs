using Aurila.Components;
using Aurila.Components.Controls;
using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout.Alignments;

internal sealed class TopAlignment : IAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Box)
            {
                builder.Add("au-box-align-top-center");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;

            if (parent is Box)
            {
                builder.Add("au-box-item-top-center");
            }
        }
    }

    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Row)
            {
                builder.Add("align-items", "flex-start");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;

            if (parent is Row)
            {
                builder.Add("align-self", "flex-start");
            }
        }
    }
}
