namespace Aurila.Components.Controls;

public class TopAppBar : ControlBase<TopAppBar>
{
    [Parameter]
    public RenderFragment? NavigationIcon { get; set; }

    [Parameter]
    public RenderFragment? Title { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-top-app-bar");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddContent(3, NavigationIcon);
            builder.AddContent(4, Title);
            builder.AddContent(5, Actions);
        }
        builder.CloseElement();
    }
}
