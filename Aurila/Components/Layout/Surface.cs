using Aurila.Design;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Layout;

public class Surface : ControlBase<Surface>
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Background { get; set; }
    
    [Parameter] public string? Color { get; set; }
    
    [Parameter] public string? Border { get; set; }

    [Parameter] public IShape? Shape { get; set; }

    [Parameter] public EventCallback Clicked { get; set; }

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

        if(Clicked.HasDelegate)
        {
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, OnClickedAsync));
        }

        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }

    private async Task OnClickedAsync()
    {
        await Clicked.InvokeAsync();
    }
}
