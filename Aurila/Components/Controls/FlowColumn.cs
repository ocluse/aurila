using Aurila.Contracts.Design;

namespace Aurila.Components.Controls;

public class FlowColumn : FlowLayoutBase<FlowColumn>
{
    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-flow-column");
        VerticalArrangement?.BuildClass(Axis.Vertical, this, builder);
        HorizontalArrangement?.BuildClass(Axis.Horizontal, this, builder);
        ItemAlignment?.BuildClass(LayoutScope.Children, this, builder);
    }

    protected override void BuildStyle(StyleBuilder builder)
    {
        base.BuildStyle(builder);
        VerticalArrangement?.BuildStyle(Axis.Vertical, this, builder);
        HorizontalArrangement?.BuildStyle(Axis.Horizontal, this, builder);
        ItemAlignment?.BuildStyle(LayoutScope.Children, this, builder);
    }
}
