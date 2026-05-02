namespace Aurila.Contracts.Layout;

public interface ICollectionView<T>
{
    IEnumerable<T>? Items { get; }

    RenderFragment<T>? ItemTemplate { get; }

    Func<T, string>? ToStringFunc { get; }
}