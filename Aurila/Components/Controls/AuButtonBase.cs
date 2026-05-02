using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuButtonBase<TControl> : AuClickableBase<TControl>
    where TControl : AuButtonBase<TControl>
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