namespace Aurila.Design.Modifiers;

public class MarginModifier(PaddingValues values) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        if (values.Top.HasValue)
        {
            builder.Add($"margin-top", values.Top.ToString());
        }
        if (values.Bottom.HasValue)
        {
            builder.Add("margin-bottom", values.Bottom.ToString());
        }
        if (values.Right.HasValue)
        {
            builder.Add("margin-right", values.Right.ToString());
        }
        if (values.Left.HasValue)
        {
            builder.Add("margin-left", values.Left.ToString());
        }

        builder.Add("box-sizing", "border-box");
    }
}
