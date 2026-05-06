namespace Aurila.Design.Modifiers;

internal class OffsetModifier(CssLength x, CssLength y) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("transform", $"translate({x}, {y})");
    }
}
