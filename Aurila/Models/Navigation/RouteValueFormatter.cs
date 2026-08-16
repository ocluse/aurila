using System.ComponentModel;
using System.Globalization;

namespace Aurila.Models.Navigation;

/// <summary>
/// Converts values to and from their URL spelling.
/// </summary>
/// <remarks>
/// Always invariant. A URL is shared, bookmarked and reopened on other machines, so a value written
/// under one locale has to read back identically under another — <c>?ratio=1,5</c> written in de-DE
/// would otherwise parse as 15 in en-US.
/// </remarks>
public static class RouteValueFormatter
{
    public static string Format(object? value) => value switch
    {
        null => string.Empty,
        string s => s,
        bool b => b ? "true" : "false",
        DateTime d => d.ToString("O", CultureInfo.InvariantCulture),
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly t => t.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    public static bool TryParse(Type targetType, string raw, out object? value)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying == typeof(string))
        {
            value = raw;
            return true;
        }

        if (raw.Length == 0)
        {
            value = targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;

            return true;
        }

        var converter = TypeDescriptor.GetConverter(underlying);

        if (converter is null || !converter.CanConvertFrom(typeof(string)))
        {
            value = null;
            return false;
        }

        try
        {
            value = converter.ConvertFromInvariantString(raw);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or NotSupportedException)
        {
            value = null;
            return false;
        }
    }
}
