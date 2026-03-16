namespace Aurila.Components.Controls;

public class Chip : ControlBase<Chip>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-chip")
            .AddIf(Selected, "au-chip--selected")
            .AddIf(Disabled, "au-chip--disabled");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "div");
        {
            builder.AddMultipleAttributes(2, GetAppliedAttributes());
            builder.AddContent(3, ChildContent);
        }
        builder.CloseElement();
    }
}
