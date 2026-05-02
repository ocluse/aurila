using Aurila.Components;
using Aurila.Contracts.Design.Appearance;

namespace Aurila.Design.Appearance;

public class SimpleStaticAppearance<T>(string? style, string? classNames) : IStaticAppearance<T>
    where T : AuControlBase<T>
{
    public string? Class => classNames;

    public string? Style => style;
}
