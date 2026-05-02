using Aurila.Design;

namespace Aurila.Components.Layout;

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
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "au-top-app-bar__navigation-icon");
            {
                builder.AddContent(5, NavigationIcon);
            }
            builder.CloseElement();
            builder.OpenElement(6, "div");
            {
                builder.AddAttribute(7, "class", "au-top-app-bar__title");
                builder.AddContent(8, Title);
            }
            builder.CloseElement();

            builder.OpenElement(9, "div");
            {
                builder.AddAttribute(10, "class", "au-top-app-bar__actions");
                builder.AddContent(11, Actions);
            }
            builder.CloseElement();
        }
        builder.CloseElement();
    }
}
