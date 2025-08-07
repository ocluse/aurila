namespace Aurila.Components.Controls;

public class Row : ControlBase<Row>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddContent(2, ChildContent);
        }
        builder.CloseElement();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-row");
    }
}
