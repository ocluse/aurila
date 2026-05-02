using Aurila.Components;

namespace Aurila.Contracts.Design.Appearance;

public interface IStaticAppearance<T> : IAppearance<T>
    where T : AuControlBase<T>
{
    string? Class { get; }

    string? Style { get; }
}
