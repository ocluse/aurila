using System.Globalization;

namespace Aurila.Design.Modifiers;

internal class OpacityModifier(double opacity) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("opacity", opacity.ToString(CultureInfo.InvariantCulture));
    }
}
