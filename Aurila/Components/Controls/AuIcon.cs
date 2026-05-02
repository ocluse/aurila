using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuIcon : AuControlBase<AuIcon>, IHasMargin
{
    [Parameter]
    [EditorRequired]
    public string? Content { get; set; }

    [Parameter]
    public IIconPainter? Painter { get; set; }

    [Parameter]
    public string? Color { get; set; }

    [Parameter]
    public CssLength Size { get; set; } = 24;

    [Parameter]
    public double? StrokeWidth { get; set; }

    [Parameter]
    public CssUnit SizeUnit { get; set; } = CssUnit.Pixels;

    [Parameter]
    public CssUnit StrokeWidthUnit { get; set; } = CssUnit.Pixels;

    [Parameter]
    public bool Fill { get; set; }
    
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
    
    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-icon");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        if (Content.IsNotEmpty())
        {
            MarkupString content = new(Content);
            builder.OpenElement(1, "svg");
            {
                builder.AddMultipleAttributes(2, GetAppliedAttributes());
                builder.AddContent(3, content);
            }
            builder.CloseElement();
        }
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        if (Color.IsNotEmpty())
        {
            builder.Add("color", Color);
        }
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);
        string iconSize = Size.ToString();
        string strokeWidth = (StrokeWidth ?? 2).ToCssValue(StrokeWidthUnit);

        attributes["height"] = iconSize;
        attributes["width"] = iconSize;
        attributes["xmlns"] = "http://www.w3.org/2000/svg";
        attributes["viewBox"] = "0 0 24 24";
        attributes["stroke-width"] = strokeWidth;
        attributes["stroke"] = Color ?? "currentColor";

        if (!Fill)
        {
            attributes["fill"] = "none";
        }

        var effectivePainter = Painter ?? AppearanceProvider?.IconPainter;

        effectivePainter?.BuildAttributes(this, attributes);
    }
}
