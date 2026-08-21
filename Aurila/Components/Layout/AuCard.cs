using Aurila.Design;

namespace Aurila.Components.Layout;

public class AuCard : AuInteractiveContainerBase<AuCard>, IHasMargin
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    [Parameter]
    public string? HeaderClass { get; set; }

    [Parameter]
    public RenderFragment? FooterContent { get; set; }

    [Parameter]
    public string? FooterClass { get; set; }

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

    protected override void BuildClass(ClassBuilder classBuilder)
    {
        base.BuildClass(classBuilder);
        classBuilder.Add("au-card");
    }

    protected override void BuildContainerContent(RenderTreeBuilder builder)
    {
        if (HeaderContent != null)
        {
            builder.OpenElement(0, "div");
            {
                builder.AddAttribute(1, "class", $"au-card__header {HeaderClass}".Trim());
                builder.AddContent(2, HeaderContent);
            }
            builder.CloseElement();
        }

        builder.OpenElement(3, "div");
        {
            builder.AddAttribute(4, "class", "au-card__content");
            builder.AddContent(5, ChildContent);
        }
        builder.CloseElement();

        if (FooterContent != null)
        {
            builder.OpenElement(6, "div");
            {
                builder.AddAttribute(7, "class", $"au-card__footer {FooterClass}".Trim());
                builder.AddContent(8, FooterContent);
            }
            builder.CloseElement();
        }
    }
}
