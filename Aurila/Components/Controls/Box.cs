using Aurila.Contracts.Components;
using Aurila.Contracts.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurila.Components.Controls;

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