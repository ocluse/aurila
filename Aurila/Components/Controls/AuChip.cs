using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuChip : AuControlBase<AuChip>, IHasMargin
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

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

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-chip")
            .AddIf(Selected, "au-chip--selected")
            .AddIf(Disabled, "au-chip--disabled");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddContent(3, ChildContent);
        }
        builder.CloseElement();
    }
}
