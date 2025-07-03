
using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Controls;

public class Button : ButtonBase<Button>
{
    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-button");
    }
}

public class IconButton : ButtonBase<IconButton>
{
    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-icon-button");
    }
}

public class ButtonBase<TControl> : ClickableBase<TControl>
    where TControl : ButtonBase<TControl>
{
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-button-base");
    }
}