using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Components.Controls;

public class Column : ControlBase<Column>, ILayoutParent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IArrangement? VerticalArrangement { get; set; }

    [Parameter]
    public IAlignment? HorizontalAlignment { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        LayoutRenderingUtility.Render(this, builder);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-column");
        VerticalArrangement?.BuildClass(this, builder);
        HorizontalAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        VerticalArrangement?.BuildStyle(this, builder);
        HorizontalAlignment?.BuildStyle(LayoutScope.Children, this, builder);
    }
}
