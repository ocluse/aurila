using Aurila.Contracts.Design;

namespace Aurila.Modifiers;

public static class ModifierExtensions
{
    public static ModifiersBuilder Modifier => new();

    public static ModifiersBuilder FillMaxSize(this ModifiersBuilder builder)
    {
        return builder.Add(new FillMaxSizeModifier());
    }

    public static ModifiersBuilder Padding(this ModifiersBuilder builder, CssLength all)
    {
        return builder.Add(new PaddingModifier(all));
    }

    public static ModifiersBuilder Padding(this ModifiersBuilder builder, CssLength? vertical = null, CssLength? horizontal = null)
    {
        return builder.Add(new PaddingModifier(PaddingValues.Symmetric(vertical, horizontal)));
    }

    public static ModifiersBuilder Padding(this ModifiersBuilder builder,
        CssLength? top = null,
        CssLength? right = null,
        CssLength? bottom = null,
        CssLength? left = null)
    {
        return builder.Add(new PaddingModifier(new PaddingValues
        {
            Top = top,
            Right = right,
            Bottom = bottom,
            Left = left
        }));
    }

    public static ModifiersBuilder Margin(this ModifiersBuilder builder, CssLength all)
    {
        return builder.Add(new MarginModifier(all));
    }

    public static ModifiersBuilder Margin(this ModifiersBuilder builder, CssLength? vertical = null, CssLength? horizontal = null)
    {
        return builder.Add(new MarginModifier(PaddingValues.Symmetric(vertical, horizontal)));
    }

    public static ModifiersBuilder Margin(this ModifiersBuilder builder,
        CssLength? top = null,
        CssLength? right = null,
        CssLength? bottom = null,
        CssLength? left = null)
    {
        return builder.Add(new MarginModifier(new PaddingValues
        {
            Top = top,
            Right = right,
            Bottom = bottom,
            Left = left
        }));
    }

    public static ModifiersBuilder Align(this ModifiersBuilder builder, IAlignment alignment)
    {
        return builder.Add(new AlignModifier(alignment));
    }

    public static ModifiersBuilder Offset(this ModifiersBuilder builder, CssLength x, CssLength y)
    {
        return builder.Add(new OffsetModifier(x, y));
    }

    public static ModifiersBuilder Clickable(this ModifiersBuilder builder, Action execute)
    {
        return builder.Add(new ClickableModifier(execute));
    }

    public static ModifiersBuilder Clickable(this ModifiersBuilder builder, Func<Task> executeAsync)
    {
        return builder.Add(new ClickableModifier(executeAsync));
    }

    public static ModifiersBuilder Clickable(this ModifiersBuilder builder, EventCallback callback)
    {
        return builder.Add(new ClickableModifier(callback));
    }

    public static ModifiersBuilder Weight(this ModifiersBuilder builder, double weight)
    {
        return builder.Add(new WeightModifier(weight));
    }

    public static ModifiersBuilder Color(this ModifiersBuilder builder, string color)
    {
        return builder.Add(new ColorModifier(color));
    }
}
