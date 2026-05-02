namespace Aurila.Design;

public static class StylingUtility
{
    public static void BuildClass(this ComponentBase component, ClassBuilder builder)
    {
        if(component is IHasShape hasShape && hasShape.Shape != null)
        {
            hasShape.Shape.BuildClass(component, builder);
        }
    }

    public static void BuildStyle(this ComponentBase component, StyleBuilder builder)
    {
        if (component is IHasPadding hasPadding)
        {
            if (hasPadding.Padding.HasValue)
            {
                builder.Add("padding", hasPadding.Padding.Value.ToString());
            }
            if (hasPadding.PaddingHorizontal.HasValue)
            {
                builder.Add("padding-left", hasPadding.PaddingHorizontal.Value.ToString());
                builder.Add("padding-right", hasPadding.PaddingHorizontal.Value.ToString());
            }
            if (hasPadding.PaddingVertical.HasValue)
            {
                builder.Add("padding-top", hasPadding.PaddingVertical.Value.ToString());
                builder.Add("padding-bottom", hasPadding.PaddingVertical.Value.ToString());
            }
            if (hasPadding.PaddingTop.HasValue)
            {
                builder.Add("padding-top", hasPadding.PaddingTop.Value.ToString());
            }
            if (hasPadding.PaddingBottom.HasValue)
            {
                builder.Add("padding-bottom", hasPadding.PaddingBottom.Value.ToString());
            }
            if (hasPadding.PaddingLeft.HasValue)
            {
                builder.Add("padding-left", hasPadding.PaddingLeft.Value.ToString());
            }
            if (hasPadding.PaddingRight.HasValue)
            {
                builder.Add("padding-right", hasPadding.PaddingRight.Value.ToString());
            }
        }

        if (component is IHasMargin hasMargin)
        {
            if (hasMargin.Margin.HasValue)
            {
                builder.Add("margin", hasMargin.Margin.Value.ToString());
            }
            if (hasMargin.MarginHorizontal.HasValue)
            {
                builder.Add("margin-left", hasMargin.MarginHorizontal.Value.ToString());
                builder.Add("margin-right", hasMargin.MarginHorizontal.Value.ToString());
            }
            if (hasMargin.MarginVertical.HasValue)
            {
                builder.Add("margin-top", hasMargin.MarginVertical.Value.ToString());
                builder.Add("margin-bottom", hasMargin.MarginVertical.Value.ToString());
            }
            if (hasMargin.MarginTop.HasValue)
            {
                builder.Add("margin-top", hasMargin.MarginTop.Value.ToString());
            }
            if (hasMargin.MarginBottom.HasValue)
            {
                builder.Add("margin-bottom", hasMargin.MarginBottom.Value.ToString());
            }
            if (hasMargin.MarginLeft.HasValue)
            {
                builder.Add("margin-left", hasMargin.MarginLeft.Value.ToString());
            }
            if (hasMargin.MarginRight.HasValue)
            {
                builder.Add("margin-right", hasMargin.MarginRight.Value.ToString());
            }
        }

        if (component is IHasBackground hasBackground)
        {
            if (!string.IsNullOrWhiteSpace(hasBackground.Background))
            {
                builder.Add("background", hasBackground.Background);
            }
        }

        if (component is IHasColor hasColor)
        {
            if (!string.IsNullOrWhiteSpace(hasColor.Color))
            {
                builder.Add("color", hasColor.Color);
            }
        }

        if (component is IHasSize hasSize)
        {
            if (hasSize.Width.HasValue)
            {
                builder.Add("width", hasSize.Width.Value.ToString());
            }

            if (hasSize.Height.HasValue)
            {
                builder.Add("height", hasSize.Height.Value.ToString());
            }

            if (hasSize.MinWidth.HasValue)
            {
                builder.Add("min-width", hasSize.MinWidth.Value.ToString());
            }

            if (hasSize.MaxWidth.HasValue)
            {
                builder.Add("max-width", hasSize.MaxWidth.Value.ToString());
            }
        }

        if (component is IHasBorder hasBorder)
        {
            if (hasBorder.Border.IsNotEmpty())
            {
                builder.Add("border", hasBorder.Border);
            }

            if (hasBorder.BorderColor.IsNotEmpty())
            {
                builder.Add("border-color", hasBorder.BorderColor);
            }

            if (hasBorder.BorderWidth.HasValue)
            {
                builder.Add("border-width", hasBorder.BorderWidth.Value.ToString());
            }
        }

        if (component is IHasShape hasShape && hasShape.Shape != null)
        {
            hasShape.Shape.BuildStyle(component, builder);
        }
    }
}
