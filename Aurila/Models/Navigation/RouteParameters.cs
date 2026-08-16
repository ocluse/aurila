using System.Diagnostics.CodeAnalysis;

namespace Aurila.Models.Navigation;

public class RouteParameters(IReadOnlyDictionary<string, string> parameters, IReadOnlyDictionary<string, string> queryParameters)
{
    private static readonly Dictionary<string, string> _noValues = new(StringComparer.OrdinalIgnoreCase);

    public static RouteParameters Empty { get; } = new(_noValues, _noValues);

    public string? Get(string name)
        => TryGetRaw(name, out var value) ? value : null;

    public bool Exists(string name)
    {
        return parameters.ContainsKey(name) || queryParameters.ContainsKey(name);
    }

    public string GetRequired(string name)
    {
        if (!TryGetRaw(name, out var value))
        {
            throw new KeyNotFoundException($"Required parameter '{name}' not found.");
        }

        return value;
    }

    public T? Get<T>(string name)
        => TryGet<T>(name, out var value) ? value : default;

    public T GetRequired<T>(string name)
    {
        if (!TryGet<T>(name, out var value))
        {
            throw new KeyNotFoundException($"Required parameter '{name}' not found or could not be converted to type '{typeof(T).FullName}'.");
        }
        return value!;
    }

    public object? Get(Type targetType, string name)
    {
        if (!TryGetRaw(name, out var rawValue))
        {
            throw new KeyNotFoundException($"Required parameter '{name}' not found.");
        }
        return ConvertOrThrow(targetType, name, rawValue);
    }

    public object GetRequired(Type targetType, string name)
    {
        if (!TryGetRaw(name, out var rawValue))
        {
            throw new KeyNotFoundException($"Required parameter '{name}' not found.");
        }
        return ConvertOrThrow(targetType, name, rawValue)!;
    }

    public bool TryGet<T>(string name, out T? value)
    {
        if (TryGet(typeof(T), name, out var converted))
        {
            value = (T?)converted;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Reads a parameter, returning <see langword="false"/> when it is absent or cannot be converted.
    /// </summary>
    /// <remarks>
    /// Query parameters are not constrained by the route template, so any value at all can arrive
    /// from a hand-typed or stale URL. Conversion failure is an expected condition here, not a fault.
    /// </remarks>
    public bool TryGet(Type targetType, string name, out object? value)
    {
        if (TryGetRaw(name, out var rawValue))
        {
            return RouteValueFormatter.TryParse(targetType, rawValue, out value);
        }

        value = null;
        return false;
    }

    private bool TryGetRaw(string name, [NotNullWhen(true)] out string? value)
    {
        if (parameters.TryGetValue(name, out value))
        {
            return true;
        }
        if (queryParameters.TryGetValue(name, out value))
        {
            return true;
        }

        value = null;

        return false;
    }

    private static object? ConvertOrThrow(Type targetType, string name, string rawValue)
    {
        if (RouteValueFormatter.TryParse(targetType, rawValue, out var value))
        {
            return value;
        }

        throw new FormatException(
            $"Failed to convert parameter '{name}' with value '{rawValue}' to type '{targetType.FullName}'.");
    }

}
