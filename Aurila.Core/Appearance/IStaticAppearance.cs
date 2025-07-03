using Aurila.Components;

namespace Aurila.Appearance;

public interface IStaticAppearance<T> : IAppearance<T>
    where T : ControlBase<T>
{
    string? Class { get; }

    string? Style { get; }
}
