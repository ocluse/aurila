using Aurila.Components;
using Aurila.Contracts.Modifiers;

namespace Aurila.Modifiers;

public class MarginModifier(PaddingValues values) : IModifier
{
    public void BuildClass(ComponentBase component, ClassBuilder builder)
    {
        // No class to add for this modifier
    }
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        builder.Add("margin-top", values.Top.ToString());
        builder.Add("margin-right", values.Right.ToString());
        builder.Add("margin-bottom", values.Bottom.ToString());
        builder.Add("margin-left", values.Left.ToString());

        builder.Add("box-sizing", "border-box");
    }
}
