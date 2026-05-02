using Aurila.Components;
using Aurila.Contracts.Appearance;

namespace Aurila.Design.Appearance;

public class SimpleStaticAppearance<T>(string? style, string? classNames) : IStaticAppearance<T>
    where T : ControlBase<T>
{
    public string? Class => classNames;

    public string? Style => style;
}
