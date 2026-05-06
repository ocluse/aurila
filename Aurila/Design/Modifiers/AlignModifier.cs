using Aurila.Contracts.Layout;
using Aurila.Enums.Layout;

namespace Aurila.Design.Modifiers;

internal class AlignModifier(IAlignment alignment) : IClassModifier, IStyleModifier
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        alignment.BuildClass(LayoutScope.Self, component, builder);
    }

    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        alignment.BuildStyle(LayoutScope.Self, component, builder);
    }
}
