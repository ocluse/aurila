using Aurila.Contracts.Design;
using Aurila.Design;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Layout;

public class AuSurface : AuControlBase<AuSurface>, IHasMargin, IHasShape, IHasBorder, IHasPadding, IHasBackground, IHasColor
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Background { get; set; }
    
    [Parameter] public string? Color { get; set; }
    
    [Parameter] public string? Border { get; set; }

    [Parameter] public IShape? Shape { get; set; }

    [Parameter] public EventCallback Clicked { get; set; }

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
    public string? BorderColor { get; set; }

    [Parameter]
    public CssLength? BorderWidth { get; set; }

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
        builder.Add("au-surface");
        builder.AddIf(Clicked.HasDelegate, "au-clickable");
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
