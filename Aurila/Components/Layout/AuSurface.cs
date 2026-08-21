using Aurila.Contracts.Design;
using Aurila.Design;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Layout;

public class AuSurface : AuInteractiveContainerBase<AuSurface>, IHasMargin, IHasShape, IHasBorder, IHasPadding, IHasBackground, IHasColor
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string? Background { get; set; }
    
    [Parameter] public string? Color { get; set; }
    
    [Parameter] public string? Border { get; set; }

    [Parameter] public IShape? Shape { get; set; }

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
    }

    protected override void BuildContainerContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }
}
