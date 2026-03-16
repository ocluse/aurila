namespace Aurila.Components;

public class StyleBuilder
{
    private readonly Dictionary<string, string> _styles = [];

    public string Build()
    {
        return string.Join("; ", _styles.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + ";";
    }

    public StyleBuilder Add(string? key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
        {
            _styles[key] = value;
        }
        return this;
    }

    public StyleBuilder AddIf(bool condition, string? key, string? value)
    {
        if (condition)
        {
            Add(key, value);
        }
        return this;
    }

    public StyleBuilder AddIfElse(bool condition, string? ifKey, string? ifValue, string? elseKey, string? elseValue)
    {
        if (condition)
        {
            Add(ifKey, ifValue);
        }
        else
        {
            Add(elseKey, elseValue);
        }
        return this;
    }
    public StyleBuilder AddIfNot(bool condition, string? key, string? value)
    {
        if (!condition)
        {
            Add(key, value);
        }
        return this;
    }

    public StyleBuilder AddRange(IDictionary<string, string> styles)
    {
        foreach (var kvp in styles)
        {
            Add(kvp.Key, kvp.Value);
        }
        return this;
    }
}
