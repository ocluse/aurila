using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuChip : AuClickableBase<AuChip>, IHasMargin
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public bool Selectable { get; set; }

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

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-chip")
            .AddIf(Selected, "au-chip--selected")
            .AddIf(Disabled, "au-chip--disabled");
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);

        if (RendersAsLink)
        {
            if (Selected)
            {
                attributes["aria-current"] = "page";
            }
        }
        else if (Selectable || Selected)
        {
            attributes["aria-pressed"] = Selected ? "true" : "false";
        }
    }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }
}
