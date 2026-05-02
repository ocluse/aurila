using Aurila.Components.Layout.Internal;
using Aurila.Contracts.Layout;
using Aurila.Design;
using Aurila.Enums.Layout;

namespace Aurila.Components.Layout;

public class Box : ControlBase<Box>, ILayoutParent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IAlignment? ContentAlignment { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        LayoutRenderingUtility.Render(this, builder);
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);

        builder.Add("au-box");

        ContentAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);

        ContentAlignment?.BuildStyle(LayoutScope.Self, this, builder);
    }
}