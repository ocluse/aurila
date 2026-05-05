using Aurila.Contracts.Design;
using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuText : AuControlBase<AuText>, IHasMargin, IHasPadding
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public ITextStyle? TextStyle { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public string? FontFamily { get; set; }

    [Parameter]
    public CssUnit? FontSize { get; set; }

    [Parameter]
    public TextAlign? Align { get; set; }

    [Parameter]
    public FontWeight? FontWeight { get; set; }

    [Parameter]
    public TextDecoration? Decoration { get; set; }

    [Parameter]
    public TextTransform? TextTransform { get; set; }

    [Parameter]
    public int? MaxLines { get; set; }

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

    [Parameter]
    public CssLength? Padding { get; set; }

    [Parameter]
    public CssLength? PaddingHorizontal { get; set; }

    [Parameter]
    public CssLength? PaddingVertical { get; set; }

    [Parameter]
    public CssLength? PaddingTop { get; set; }

    [Parameter]
    public CssLength? PaddingBottom { get; set; }

    [Parameter]
    public CssLength? PaddingRight { get; set; }

    [Parameter]
    public CssLength? PaddingLeft { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);

        TextStyle?.BuildClass(this, builder);

        builder.Add("au-text");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        TextStyle?.BuildStyle(this, builder);

        if(FontFamily.IsNotWhiteSpace())
        {
            builder.Add("font-family", FontFamily);
        }

        if (FontSize.HasValue)
        {
            string fontSizeValue = FontSize.Value.ToString();
            builder.Add("font-size", fontSizeValue);
        }

        if (Align.HasValue)
        {
            string alignValue = Align.Value.ToCssValue();
            builder.Add("text-align", alignValue);
        }

        if (FontWeight.HasValue)
        {
            string fontWeightValue = FontWeight.Value.ToCssValue();
            builder.Add("font-weight", fontWeightValue);
        }

        if (Color.IsNotWhiteSpace())
        {
            builder.Add("color", Color);
        }

        if (TextTransform.HasValue)
        {
            string textTransformValue = TextTransform.Value.ToCssValue();   
            builder.Add("text-transform", textTransformValue);
        }

        if (Decoration.HasValue)
        {
            string textDecorationValue = Decoration.Value.ToCssValue();
            builder.Add("text-decoration", textDecorationValue);
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
        string elementName = TextStyle?.ElementName ?? "div";

        builder.OpenElement(0, elementName);
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
            builder.AddContent(2, ChildContent);
        }
        builder.CloseElement();
    }
}
