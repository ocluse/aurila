namespace Aurila.Design.Modifiers;

internal class ColorModifier(string color) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("color", color);
    }
}