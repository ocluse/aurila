using Aurila.Design;

namespace Aurila.Components.Controls;

public class AuButtonBase<TControl> : AuClickableBase<TControl>, 
    IHasPadding, 
    IHasMargin, 
    IHasSize
    where TControl : AuButtonBase<TControl>
{
    [Parameter]
    [EditorRequired]
    public RenderFragment? ChildContent { get; set; }

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
    public CssLength? Width { get; set; }
    
    [Parameter]
    public CssLength? Height { get; set; }
    
    [Parameter]
    public CssLength? MinWidth { get; set; }
    
    [Parameter]
    public CssLength? MaxWidth { get; set; }
    
    [Parameter]
    public CssLength? MinHeight { get; set; }
    
    [Parameter]
    public CssLength? MaxHeight { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-button-base");
    }
}