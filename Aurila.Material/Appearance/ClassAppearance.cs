using Aurila.Components;

namespace Aurila.Material.Appearance;

/// <summary>An appearance that only contributes class names; the stylesheet does the rest.</summary>
public sealed class ClassAppearance<T>(params string[] classes) : IBuildingAppearance<T>
    where T : AuControlBase<T>
{
    public void BuildClass(T control, ClassBuilder builder) => builder.AddRange(classes);

    public void BuildStyle(T control, StyleBuilder builder)
    {
    }
}
