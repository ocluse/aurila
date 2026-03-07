using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Components.Controls;

public class Row : ControlBase<Row>, ILayoutParent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IArrangement? HorizontalArrangement { get; set; }

    [Parameter]
    public IAlignment? VerticalAlignment { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        LayoutRenderingUtility.Render(this, builder);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-row");
        VerticalAlignment?.BuildClass(LayoutScope.Children, this, builder);
        HorizontalArrangement?.BuildClass(this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        VerticalAlignment?.BuildStyle(LayoutScope.Children, this, builder);
        HorizontalArrangement?.BuildStyle(this, builder);
    }
}
