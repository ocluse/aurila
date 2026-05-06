using System.Globalization;

namespace Aurila.Design.Modifiers;

internal class WeightModifier(double weight) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("flex", weight.ToString(CultureInfo.InvariantCulture));
    }
}
