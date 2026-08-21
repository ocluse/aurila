using System.Text;
using Aurila.Material.Colors;
using Aurila.Material.Tokens;

namespace Aurila.Material;

/// <summary>A generated scheme pair plus the CSS that publishes it as custom properties.</summary>
public sealed class MaterialTheme
{
    public MaterialTheme(MaterialThemeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        Palettes = MaterialPalettes.FromSeed(options.Seed, options.Variant);
        FontFamily = options.FontFamily;

        ColorScheme light = ColorScheme.Light(Palettes);
        ColorScheme dark = ColorScheme.Dark(Palettes);

        Light = options.Light?.Invoke(light) ?? light;
        Dark = options.Dark?.Invoke(dark) ?? dark;
    }

    public MaterialPalettes Palettes { get; }

    public ColorScheme Light { get; }

    public ColorScheme Dark { get; }

    public string FontFamily { get; }

    public ColorScheme For(ThemeMode mode) => mode == ThemeMode.Dark ? Dark : Light;

    /// <summary>
    /// The stylesheet publishing this theme's roles. In <see cref="ThemeMode.System"/> both schemes are
    /// emitted and the browser picks, so the preference is honoured without a round trip to .NET.
    /// </summary>
    public string BuildCss(ThemeMode mode)
    {
        StringBuilder sb = new();

        sb.Append(":root{");
        AppendRoles(sb, mode == ThemeMode.Dark ? Dark : Light);
        sb.Append("--md-sys-typescale-font:").Append(Sanitize(FontFamily)).Append(';');
        sb.Append("color-scheme:").Append(mode switch
        {
            ThemeMode.Light => "light",
            ThemeMode.Dark => "dark",
            _ => "light dark",
        }).Append(';');
        sb.Append('}');

        if (mode == ThemeMode.System)
        {
            sb.Append("@media(prefers-color-scheme:dark){:root{");
            AppendRoles(sb, Dark);
            sb.Append("}}");
        }

        return sb.ToString();
    }

    private static void AppendRoles(StringBuilder sb, ColorScheme scheme)
    {
        foreach ((string role, string value) in scheme.EnumerateRoles())
        {
            sb.Append(MaterialColors.CssVariableName(role)).Append(':').Append(Sanitize(value)).Append(';');
        }
    }

    // Values reach a <style> element, so a stray brace or angle bracket would end the rule early.
    private static string Sanitize(string value)
    {
        foreach (char c in value)
        {
            if (c is '<' or '>' or '{' or '}' or ';')
            {
                return value.Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("{", string.Empty)
                    .Replace("}", string.Empty)
                    .Replace(";", string.Empty);
            }
        }

        return value;
    }
}
