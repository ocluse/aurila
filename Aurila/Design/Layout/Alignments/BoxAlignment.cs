using Aurila.Components.Layout;
using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Alignments;

internal class BoxAlignment(string vertical, string horizontal) : IBidirectionalAlignment
{
    private readonly string _parentClass = $"au-box-align-{vertical}-{horizontal}";
    private readonly string _childClass = $"au-box-item-{vertical}-{horizontal}";

    public void BuildClass(LayoutScope scope, ComponentBase component, ClassBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuBox)
            {
                builder.Add(_parentClass);
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuBox)
            {
                builder.Add(_childClass);
            }
        }
    }
    public void BuildStyle(LayoutScope scope, ComponentBase component, StyleBuilder builder)
    {
        if (scope is LayoutScope.Children)
        {
            if (component is AuGrid)
            {
                builder.Add("place-items", $"{Map(vertical)} {Map(horizontal)}");
            }
        }
        else if (scope is LayoutScope.Self && component is ILayoutChild layoutChild)
        {
            var parent = layoutChild.Parent;
            if (parent is AuGrid)
            {
                builder.Add("place-self", $"{Map(vertical)} {Map(horizontal)}");
            }
        }
    }

    private string Map(string value) => value switch
    {
        "top" => "start",
        "bottom" => "end",
        "start" => "start",
        "end" => "end",
        "center" => "center",
        _ => "start"
    };
}