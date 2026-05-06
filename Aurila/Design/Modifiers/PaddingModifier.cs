namespace Aurila.Design.Modifiers;

internal class PaddingModifier(PaddingValues values) : IStyleModifier
{
    public void BuildStyle(ComponentBase component, StyleBuilder builder)
    {
        if(values.Top.HasValue)
        {
            builder.Add("padding-top", values.Top.ToString());
        }

        if (values.Bottom.HasValue)
        {
            builder.Add("padding-bottom", values.Bottom.ToString());
        }

        if (values.Right.HasValue)
        {
            builder.Add("padding-right", values.Right.ToString());
        }

        if(values.Left.HasValue)
        {
            builder.Add("padding-left", values.Left.ToString());
        }

        builder.Add("box-sizing", "border-box");
    }
}
