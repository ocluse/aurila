using Aurila.Components.Layout;
using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Alignments;

internal sealed class StartAlignment : IAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        // No class;
    }
    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Column or FlowColumn)
            {
                builder.Add("align-items", "flex-start");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is Column or FlowColumn)
            {
                builder.Add("align-self", "flex-start");
            }
        }
    }
}
