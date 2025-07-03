namespace Aurila.Components.Controls;
public partial class Scaffold
{
    [Parameter]
    public RenderFragment? TopBar { get; set; }

    [Parameter]
    public string? TopBarClass { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? ChildContentClass { get; set; }

    [Parameter]
    public RenderFragment? BottomBar { get; set; }

    [Parameter]
    public string? BottomBarClass { get; set; }

    [Parameter]
    public RenderFragment? FloatingActionButton { get; set; }

    [Parameter]
    public string? FloatingActionButtonClass { get; set; }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("scaffold");
    }
}