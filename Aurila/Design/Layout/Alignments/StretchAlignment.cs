using Aurila.Components;
using Aurila.Components.Controls;
using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout.Alignments;

internal sealed class StretchAlignment : IAlignment
{
    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        // No class;
    }

    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is Row or Column)
            {
                builder.Add("align-items", "stretch");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is Row or Column)
            {
                builder.Add("align-self", "stretch");
            }
        }
    }
}
