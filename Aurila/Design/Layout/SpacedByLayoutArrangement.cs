using Aurila.Components;
using Aurila.Contracts.Design;

namespace Aurila.Design.Layout;

internal class SpacedByLayoutArrangement(
    string className,
    double spacingPx
) : IArrangement
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        builder.Add(className);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("--au-spaced-by-spacing", $"{spacingPx}px");
    }
}
