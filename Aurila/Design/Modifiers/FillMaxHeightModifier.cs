namespace Aurila.Design.Modifiers;

public class FillMaxHeightModifier(double fraction) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("height", $"{fraction * 100}%");
        builder.Add("box-sizing", "border-box");
        builder.Add("flex-shrink", "0");
        builder.Add("flex-grow", "0");
        builder.Add("flex-basis", $"{fraction * 100}%");
    }
}