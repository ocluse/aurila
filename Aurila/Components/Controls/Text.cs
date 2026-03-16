namespace Aurila.Components.Controls;
public class TextBlock : ControlBase<TextBlock>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public ITextStyle? TextStyle { get; set; }

    [Parameter]
    public IColor? Color { get; set; }

    [Parameter]
    public TextAlign? Align { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-text-block");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        if (Align.HasValue)
        {
            string alignValue = Align.Value switch
            {
                TextAlign.Start => "start",
                TextAlign.End => "end",
                TextAlign.Center => "center",
                TextAlign.Justify => "justify",
                _ => throw new InvalidOperationException($"Unsupported TextAlign value: {Align.Value}")
            };

            builder.Add("text-align", alignValue);
        }
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
