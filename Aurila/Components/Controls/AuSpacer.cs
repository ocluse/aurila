using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuSpacer : AuControlBase<AuSpacer>
{
    [Parameter]
    public double Width { get; set; }

    [Parameter]
    public double Height { get; set; }

    [Parameter]
    public double Weight { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-spacer");
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        builder.AddIf(Width > 0, "width", $"{Width}px");
        builder.AddIf(Height > 0, "height", $"{Height}px");
        builder.AddIf(Weight > 0, "flex-grow", Weight.ToString());
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        {
            builder.AddMultipleAttributes(1, GetAppliedAttributes());
        }
        builder.CloseElement();
    }
}
