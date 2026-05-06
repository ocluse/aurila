namespace Aurila.Components.Layout;
public class AuGroupBox : AuControlBase<AuGroupBox>, IHasMargin
{
    [Parameter]
    public RenderFragment? Header { get; set; }

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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            if(Header != null)
            {
                builder.OpenElement(2, "div");
                {
                    builder.AddAttribute(3, "class", "au-group-box__header");
                    builder.AddContent(4, Header);
                }
                builder.CloseElement();
            }
            
            builder.OpenElement(5, "div");
            {
                builder.AddAttribute(6, "class", "au-group-box__content");
                if (ChildContent != null)
                {
                    builder.AddContent(7, ChildContent);
                }
            }
            builder.CloseElement();

        }
        builder.CloseElement();
    }
}
