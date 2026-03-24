using Aurila.Contracts.Design;

namespace Aurila.Components.Controls;

public class FlowRow : FlowLayoutBase<FlowRow>
{
    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-flow-row");
        HorizontalArrangement?.BuildClass(Axis.Horizontal, this, builder);
        VerticalArrangement?.BuildClass(Axis.Vertical, this, builder);
        ItemAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        HorizontalArrangement?.BuildStyle(Axis.Horizontal, this, builder);
        VerticalArrangement?.BuildStyle(Axis.Vertical, this, builder);
        ItemAlignment?.BuildStyle(LayoutScope.Children, this, builder);
    }
}
