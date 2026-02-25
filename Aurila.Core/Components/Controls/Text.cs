namespace Aurila.Components.Controls;
public class Text : ControlBase<Text>
{
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-text");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddContent(2, ChildContent);
        }
        builder.CloseElement();
    }
}
