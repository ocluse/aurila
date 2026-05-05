using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuHorizontalDivider : AuControlBase<AuHorizontalDivider>, IHasMargin
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public CssLength? Width { get; set; }

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
        builder.Add("au-horizontal-divider");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        if(Color != null)
        {
            builder.Add("border-color", Color);
        }

        if(Width != null)
        {
            builder.Add("border-top-width", Width.ToString());
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "hr");
        builder.AddMultipleAttributes(2, GetAppliedAttributes());
        builder.CloseElement();
    }
}