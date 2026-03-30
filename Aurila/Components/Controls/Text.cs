namespace Aurila.Components.Controls;

public class TextBlock : ControlBase<TextBlock>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public ITextStyle? TextStyle { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public TextAlign? Align { get; set; }

    [Parameter]
    public FontWeight? FontWeight { get; set; }

    [Parameter]
    public TextTransform? TextTransform { get; set; }

    [Parameter]
    public int? MaxLines { get; set; }

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

        if (FontWeight.HasValue)
        {
            string fontWeightValue = ((int)FontWeight.Value).ToString();
            builder.Add("font-weight", fontWeightValue);
        }

        if (Color.IsNotWhiteSpace())
        {
            builder.Add("color", Color);
        }

        if (TextTransform.HasValue)
        {
            string textTransformValue = TextTransform.Value switch
            {
                Enums.TextTransform.None => "none",
                Enums.TextTransform.Uppercase => "uppercase",
                Enums.TextTransform.Lowercase => "lowercase",
                Enums.TextTransform.Capitalize => "capitalize",
                _ => throw new InvalidOperationException($"Unsupported TextTransform value: {TextTransform.Value}")
            };

            builder.Add("text-transform", textTransformValue);
        }

        if (MaxLines.HasValue && MaxLines.Value > 0)
        {
            builder.Add("display", "-webkit-box");
            builder.Add("-webkit-box-orient", "vertical");
            builder.Add("-webkit-line-clamp", MaxLines.Value.ToString());
            builder.Add("overflow", "hidden");
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
