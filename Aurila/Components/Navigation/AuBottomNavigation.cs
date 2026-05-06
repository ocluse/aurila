using Aurila.Design;

namespace Aurila.Components.Navigation;

public class AuBottomNavigation : AuControlBase<AuBottomNavigation>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "nav");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddContent(3, ChildContent);
        }
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-bottom-navigation");
    }
}
