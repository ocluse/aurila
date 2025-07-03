using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Controls;

public class BottomNavigationIcon : ClickableBase<BottomNavigationIcon>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-bottom-navigation-icon");
    }
}
