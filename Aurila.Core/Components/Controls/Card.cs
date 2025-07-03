using Microsoft.AspNetCore.Components.Rendering;

namespace Aurila.Components.Controls;

public class Card : ControlBase<Card>
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

    protected override void BuildClass(ClassBuilder classBuilder)
    {
        base.BuildClass(classBuilder);
        classBuilder.Add("au-card");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());

            if (HeaderContent != null)
            {
                builder.OpenElement(3, "div");
                {
                    builder.AddAttribute(4, "class", "au-card__header");
                    builder.AddContent(5, HeaderContent);
                }
                builder.CloseElement();
            }

            builder.OpenElement(6, "div");
            {
                builder.AddAttribute(7, "class", "au-card__content");
                builder.AddContent(8, ChildContent);
            }
            builder.CloseElement();

            if (FooterContent != null)
            {
                builder.OpenElement(9, "div");
                {
                    builder.AddAttribute(10, "class", "au-card__footer");
                    builder.AddContent(11, FooterContent);
                }
                builder.CloseElement();
            }
        }
        builder.CloseElement();
    }
}
