using Aurila.Enums.Navigation;

namespace Aurila.Models.Navigation;

/// <summary>
/// A page parameter that is kept in step with the query string in both directions.
/// </summary>
/// <remarks>
/// <para>
/// Reading the URL into a property is easy; writing a property back out is where the two can drift.
/// A holder gives the write somewhere to go: assigning <see cref="Value"/> issues a navigation, and
/// the new value arrives back through the URL. The URL therefore stays the source of truth even for
/// changes that originate in C#.
/// </para>
/// <para>
/// A value equal to <see cref="Default"/> is omitted from the URL, so <c>/orders</c> and
/// <c>/orders?page=1</c> address the same thing.
/// </para>
/// </remarks>
public sealed class QueryParam<T> : IQueryParam
{
    private readonly IEqualityComparer<T?> _comparer = EqualityComparer<T?>.Default;

    private IQueryParamWriter? _writer;
    private string _name = string.Empty;
    private T? _value;

    public QueryParam(T? defaultValue = default, NavHistory history = NavHistory.Replace)
    {
        _value = defaultValue;
        Default = defaultValue;
        History = history;
    }

    public T? Default { get; }

    /// <summary>
    /// Whether changing this parameter adds a history entry. Push makes the change back-navigable,
    /// which is usually what a user expects of a filter or a tab.
    /// </summary>
    public NavHistory History { get; }

    public T? Value
    {
        get => _value;
        set
        {
            if (_comparer.Equals(_value, value))
            {
                return;
            }

            _value = value;
            Changed?.Invoke();

            _writer?.Write(_name, Format(), History);
        }
    }

    public bool IsDefault => _comparer.Equals(_value, Default);

    /// <summary>
    /// Raised whenever the value changes, from either direction.
    /// </summary>
    public event Action? Changed;

    public static implicit operator T?(QueryParam<T> parameter) => parameter._value;

    public override string? ToString() => _value?.ToString();

    void IQueryParam.Bind(string name, IQueryParamWriter writer)
    {
        _name = name;
        _writer = writer;
    }

    void IQueryParam.ReadFrom(RouteParameters parameters)
    {
        T? incoming = parameters.TryGet<T>(_name, out var parsed) ? parsed : Default;

        if (_comparer.Equals(_value, incoming))
        {
            return;
        }

        _value = incoming;
        Changed?.Invoke();
    }

    private string? Format()
        => IsDefault ? null : RouteValueFormatter.Format(_value);
}

internal interface IQueryParam
{
    void Bind(string name, IQueryParamWriter writer);

    void ReadFrom(RouteParameters parameters);
}

internal interface IQueryParamWriter
{
    void Write(string name, string? value, NavHistory history);
}
