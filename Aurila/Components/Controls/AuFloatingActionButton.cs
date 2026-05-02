using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuFloatingActionButton : AuClickableBase<AuFloatingActionButton>, IHasMargin
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public CssLength? Margin { get; set; }

    [Parameter]
    public CssLength? MarginHorizontal { get; set; }

    [Parameter]
    public CssLength? MarginVertical { get; set; }

    [Parameter]
    public CssLength? MarginRight { get; set; }

    [Parameter]
    public CssLength? MarginLeft { get; set; }

    [Parameter]
    public CssLength? MarginTop { get; set; }

    [Parameter]
    public CssLength? MarginBottom { get; set; }

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
