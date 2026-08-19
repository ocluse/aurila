using System.Text;
using Aurila.Fluent.Tokens;

namespace Aurila.Fluent;

/// <summary>A generated Fluent brand ramp, semantic schemes, and their CSS variables.</summary>
public sealed class FluentTheme
{
    public FluentTheme(FluentThemeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Brand = FluentBrandRamp.FromSeed(options.Seed);
        FontFamily = options.FontFamily;
        Light = options.Light?.Invoke(FluentColorScheme.Light(Brand)) ?? FluentColorScheme.Light(Brand);
        Dark = options.Dark?.Invoke(FluentColorScheme.Dark(Brand)) ?? FluentColorScheme.Dark(Brand);
    }

    public FluentBrandRamp Brand { get; }
    public FluentColorScheme Light { get; }
    public FluentColorScheme Dark { get; }
    public string FontFamily { get; }

    public FluentColorScheme For(FluentThemeMode mode) => mode == FluentThemeMode.Dark ? Dark : Light;

    public string BuildCss(FluentThemeMode mode)
    {
        StringBuilder css = new();
        css.Append(":root{");
        AppendScheme(css, mode == FluentThemeMode.Dark ? Dark : Light);
        AppendBrand(css);
        css.Append("--fluent-font-family-base:").Append(Sanitize(FontFamily)).Append(';');
        css.Append("color-scheme:").Append(mode switch
        {
            FluentThemeMode.Light => "light",
            FluentThemeMode.Dark => "dark",
            _ => "light dark",
        }).Append(";}");

        if (mode == FluentThemeMode.System)
        {
            css.Append("@media(prefers-color-scheme:dark){:root{");
            AppendScheme(css, Dark);
            css.Append("}}");
        }

        return css.ToString();
    }

    private static void AppendScheme(StringBuilder css, FluentColorScheme scheme)
    {
        foreach ((string name, string value) in scheme.EnumerateRoles())
        {
            css.Append(FluentColors.CssVariableName(name)).Append(':').Append(Sanitize(value)).Append(';');
        }
    }

    private void AppendBrand(StringBuilder css)
    {
        foreach ((int shade, string value) in Brand.Enumerate())
        {
            css.Append("--fluent-brand-").Append(shade).Append(':').Append(value).Append(';');
        }
    }

    private static string Sanitize(string value) => value
        .Replace("<", string.Empty).Replace(">", string.Empty)
        .Replace("{", string.Empty).Replace("}", string.Empty).Replace(";", string.Empty);
}
