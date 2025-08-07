using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Controls;

public class BottomNavigationItem : ClickableBase<BottomNavigationItem>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-bottom-navigation-item")
            .AddIf(Selected, "au-bottom-navigation-item--selected");
    }
}
