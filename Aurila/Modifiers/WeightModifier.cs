using Aurila.Components;
using Aurila.Contracts.Modifiers;
using System.Globalization;

namespace Aurila.Modifiers;

internal class WeightModifier(double weight) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("flex", weight.ToString(CultureInfo.InvariantCulture));
    }
}
