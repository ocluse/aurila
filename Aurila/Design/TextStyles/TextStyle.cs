using Aurila.Contracts.Design;

namespace Aurila.Design.TextStyles;

public record TextStyle : ITextStyle
{
    public string? ElementName { get; set; }

    public string? FontFamily { get; set; }

    public CssLength? FontSize { get; set; }

    public FontWeight? FontWeight { get; set; }

    public TextTransform? TextTransform { get; set; }

    public TextAlign? Align { get; set; }

    public string? Color { get; set; }

    public string? Class { get; set; }

    public Action<ComponentBase, ClassBuilder>? ClassBuilder { get; set; }

    public Action<ComponentBase, StyleBuilder>? StyleBuilder { get; set; }

    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        if (Class.IsNotEmpty())
        {
            builder.Add(Class);
        }

        ClassBuilder?.Invoke(component, builder);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        if(FontFamily.IsNotWhiteSpace())
        {
            builder.Add("font-family", FontFamily);
        }

        if(FontSize.HasValue)
        {
            builder.Add("font-size", FontSize.Value.ToString());
        }

        if (FontWeight.HasValue)
        {
            builder.Add("font-weight", FontWeight.Value.ToCssValue());
        }

        if (TextTransform.HasValue)
        {
            string textTransformValue = TextTransform.Value.ToCssValue();
            builder.Add("text-transform", textTransformValue);
        }

        if (Align.HasValue)
        {
            string textAlignValue = Align.Value.ToCssValue();
            builder.Add("text-align", textAlignValue);
        }

        if(Color.IsNotWhiteSpace())
        {
            builder.Add("color", Color);
        }

        StyleBuilder?.Invoke(component, builder);
    }
}
