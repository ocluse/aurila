using Aurila.Components;
using Aurila.Components.Controls;
using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout.Alignments;

internal sealed class BottomAlignment : IAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Box)
            {
                builder.Add("au-box-align-bottom-center");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is Box)
            {
                builder.Add("au-box-align-bottom-center");
            }
        }
    }
    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Row)
            {
                builder.Add("align-items", "flex-end");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is Row)
            {
                builder.Add("align-self", "flex-end");
            }
        }
    }
}
