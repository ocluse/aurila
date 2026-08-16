using Aurila.Components;
using Aurila.Contracts.Design.Appearance;

namespace Aurila.Design.Appearance;

public class CompositeAppearance<T> : IBuildingAppearance<T>
    where T : AuControlBase<T>
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
            if (appearance is IBuildingAppearance<T> buildingAppearance)
            {
                buildingAppearance.BuildClass(control, builder);
            }
            else if (appearance is IStaticAppearance<T> staticAppearance)
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
                var styles = StylingUtility.GetStyles(staticAppearance.Style);

                foreach (var style in styles)
                {
                    builder.Add(style.Key, style.Value);
                }
            }
            else
            {
                throw new InvalidOperationException($"Appearance {appearance.GetType().Name} does not implement IBuildingAppearance<T> or IStaticAppearance<T>.");
            }
        }
    }
}
