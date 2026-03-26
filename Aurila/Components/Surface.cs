using Aurila.Contracts.Design;

namespace Aurila.Components;

public class Surface : ControlBase<Surface>
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Background { get; set; }
    [Parameter] public string? Color { get; set; }
    [Parameter] public string? Border { get; set; }

    [Parameter] public IShape? Shape { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-surface");
        Shape?.BuildClass(this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        if (Background.IsNotWhiteSpace())
            builder.Add("background", Background);

        if (Color.IsNotWhiteSpace())
            builder.Add("color", Color);

        if (Border.IsNotWhiteSpace())
            builder.Add("border", Border);

        Shape?.BuildStyle(this, builder);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddMultipleAttributes(1, GetAppliedAttributes());
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
