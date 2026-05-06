using System.ComponentModel;
using System.Globalization;

namespace Aurila.Models.Navigation;

public class RouteParameters(IReadOnlyDictionary<string, string> parameters, IReadOnlyDictionary<string, string> queryParameters)
{
    public string? Get(string name)
    {
        return parameters.TryGetValue(name, out var value) ? value : queryParameters.TryGetValue(name, out value) ? value : null;
    }

    public T? Get<T>(string name)
    {
        if (!parameters.TryGetValue(name, out var value) && !queryParameters.TryGetValue(name, out value))
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
