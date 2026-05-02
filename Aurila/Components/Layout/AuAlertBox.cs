using Aurila.Design;

namespace Aurila.Components.Layout;

public class AuAlertBox : AuControlBase<AuAlertBox>
{
    [Parameter]
    [EditorRequired]
    public RenderFragment? Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());

            builder.OpenElement(2, "div");
            {
                builder.AddAttribute(3, "class", "au-alertbox__icon");
                builder.AddContent(4, Icon);
            }
            builder.CloseElement(); //div

            builder.OpenElement(5, "div");
            {
                builder.AddAttribute(6, "class", "au-alertbox__content");
                builder.AddContent(7, ChildContent);
            }
            builder.CloseElement(); //div
        }
        builder.CloseElement(); //div
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-alertbox");
    }
}
