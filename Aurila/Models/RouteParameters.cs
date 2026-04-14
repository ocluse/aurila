using System.ComponentModel;
using System.Globalization;

namespace Aurila.Models;

public class RouteParameters
{
    private readonly IReadOnlyDictionary<string, string> _parameters;

    public RouteParameters(IReadOnlyDictionary<string, string> parameters)
    {
        _parameters = parameters;
    }

    public string? Get(string name)
    {
        return _parameters.TryGetValue(name, out var value) ? value : null;
    }

    public T? Get<T>(string name)
    {
        if (!_parameters.TryGetValue(name, out var value))
        {
            throw new KeyNotFoundException($"Route parameter '{name}' was not found.");
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                var result = converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
                return result == null ? default : (T)result;
            }
            
            throw new NotSupportedException($"No type converter exists to convert from string to {typeof(T).FullName}.");
        }
        catch (Exception ex) when (ex is not NotSupportedException)
        {
            throw new FormatException($"Failed to convert route parameter '{name}' with value '{value}' to type {typeof(T).FullName}.", ex);
        }
    }
}
