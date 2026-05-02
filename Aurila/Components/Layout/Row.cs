using Aurila.Components.Layout.Internal;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Enums.Layout;

namespace Aurila.Components.Layout;

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
        HorizontalArrangement?.BuildClass(Axis.Horizontal, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        VerticalAlignment?.BuildStyle(LayoutScope.Children, this, builder);
        HorizontalArrangement?.BuildStyle(Axis.Horizontal, this, builder);
    }
}
