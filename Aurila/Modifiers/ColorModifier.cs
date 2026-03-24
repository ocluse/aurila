using Aurila.Components;
using Aurila.Contracts.Modifiers;

namespace Aurila.Modifiers;

internal class ColorModifier(string color) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("color", color);
    }
}