using Aurila.Contracts.Components;
using Aurila.Contracts.Design;

namespace Aurila.Components.Controls;

public abstract class FlowLayoutBase<T> : ControlBase<T>, ILayoutParent
    where T : FlowLayoutBase<T>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IArrangement? HorizontalArrangement { get; set; }

    [Parameter]
    public IArrangement? VerticalArrangement { get; set; }

    [Parameter]
    public IAlignment? ItemAlignment { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        LayoutRenderingUtility.Render(this, builder);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-flow-layout");
    }
}
