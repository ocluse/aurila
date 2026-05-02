using Aurila.Components.Layout;
using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Layout.Arrangements;

internal class SpacedByArrangement(CssLength spacing, MainAxisAlignment? alignment = null) : IArrangement
{
    public void BuildClass(Axis axis, ComponentBase component, ClassBuilder builder)
    {
    }

    public void BuildStyle(Axis axis, ComponentBase component, StyleBuilder builder)
    {
        var gapProperty = axis is Axis.Horizontal ? "column-gap" : "row-gap";
        builder.Add(gapProperty, spacing.ToString());

        if (alignment.HasValue)
        {
            var alignmentProperty = ResolveAlignmentProperty(component, axis);
            var alignmentValue = ResolveAlignmentValue(axis, alignment.Value);

            builder.Add(alignmentProperty, alignmentValue);
        }
    }

    private static string ResolveAlignmentProperty(ComponentBase component, Axis axis)
    {
        if (component is FlowRow)
        {
            return axis is Axis.Horizontal ? "justify-content" : "align-content";
        }

        if (component is FlowColumn)
        {
            return axis is Axis.Vertical ? "justify-content" : "align-content";
        }

        return "justify-content";
    }

    private static string ResolveAlignmentValue(Axis axis, MainAxisAlignment alignment)
    {
        return axis switch
        {
            Axis.Horizontal => alignment switch
            {
                MainAxisAlignment.Start => "flex-start",
                MainAxisAlignment.Center => "center",
                MainAxisAlignment.End => "flex-end",
                MainAxisAlignment.Top => "flex-start",
                MainAxisAlignment.Bottom => "flex-end",
                _ => throw new InvalidOperationException($"Unsupported {nameof(MainAxisAlignment)} value: {alignment}")
            },

            Axis.Vertical => alignment switch
            {
                MainAxisAlignment.Top => "flex-start",
                MainAxisAlignment.Center => "center",
                MainAxisAlignment.Bottom => "flex-end",
                MainAxisAlignment.Start => "flex-start",
                MainAxisAlignment.End => "flex-end",
                _ => throw new InvalidOperationException($"Unsupported {nameof(MainAxisAlignment)} value: {alignment}")
            },

            _ => throw new InvalidOperationException($"Unsupported {nameof(Axis)} value: {axis}")
        };
    }
}