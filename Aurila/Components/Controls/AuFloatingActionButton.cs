using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuFloatingActionButton : AuClickableBase<AuFloatingActionButton>
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
        builder.Add("au-floating-action-button");
    }
}
