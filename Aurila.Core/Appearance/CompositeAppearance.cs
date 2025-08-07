using Aurila.Components;

namespace Aurila.Appearance;

public class CompositeAppearance<T> : IBuildingAppearance<T>
    where T : ControlBase<T>
{
    private readonly List<IAppearance<T>> _appearances = [];

    public CompositeAppearance(params IEnumerable<IAppearance<T>> appearances)
    {
        _appearances.AddRange(appearances);
    }

    public void BuildClass(T control, ClassBuilder builder)
    {
        foreach (var appearance in _appearances)
        {
            if(appearance is IBuildingAppearance<T> buildingAppearance)
            {
                buildingAppearance.BuildClass(control, builder);
            }
            else if(appearance is IStaticAppearance<T> staticAppearance)
            {
                builder.Add(staticAppearance.Class);
            }
            else
            {
                throw new InvalidOperationException($"Appearance {appearance.GetType().Name} does not implement IBuildingAppearance<T> or IStaticAppearance<T>.");
            }
        }
    }

    public void BuildStyle(T control, StyleBuilder builder)
    {
        foreach (var appearance in _appearances)
        {
            if (appearance is IBuildingAppearance<T> buildingAppearance)
            {
                buildingAppearance.BuildStyle(control, builder);
            }
            else if (appearance is IStaticAppearance<T> staticAppearance)
            {
                string? style = staticAppearance.Style;

                if (style.IsNotWhiteSpace())
                {
                    string[] tokens = style.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        var styleParts = token.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (styleParts.Length != 2)
                        {
                            continue; // Skip invalid style tokens
                        }

                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            builder.Add(styleParts[1], styleParts[2]);
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Appearance {appearance.GetType().Name} does not implement IBuildingAppearance<T> or IStaticAppearance<T>.");
            }
        }
    }
}