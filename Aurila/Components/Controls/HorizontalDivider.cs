namespace Aurila.Components.Controls;

public class HorizontalDivider : ControlBase<HorizontalDivider>
{
    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public CssLength? Width { get; set; }

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