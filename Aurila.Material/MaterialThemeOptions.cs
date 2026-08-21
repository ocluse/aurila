using Aurila.Material.Colors;

namespace Aurila.Material;

public enum ThemeMode
{
    /// <summary>Follow the operating system's light or dark preference.</summary>
    System,
    Light,
    Dark,
}

public class MaterialThemeOptions
{
    /// <summary>The brand colour the scheme is generated from.</summary>
    public string Seed { get; set; } = "#6750A4";

    public SchemeVariant Variant { get; set; } = SchemeVariant.TonalSpot;

    public ThemeMode Mode { get; set; } = ThemeMode.System;

    /// <summary>Overrides applied after generation. Return the scheme with the roles you want changed.</summary>
    /// <example><c>o.Light = s =&gt; s with { Primary = "#FF6D00" };</c></example>
    public Func<ColorScheme, ColorScheme>? Light { get; set; }

    public Func<ColorScheme, ColorScheme>? Dark { get; set; }

    /// <summary>Emitted as <c>--md-sys-typescale-font</c>. Aurila does not load the font for you.</summary>
    public string FontFamily { get; set; } = "Roboto, system-ui, -apple-system, 'Segoe UI', sans-serif";

    /// <summary>Turns off the pointer ripple, leaving the CSS-only state layers.</summary>
    public bool DisableRipple { get; set; }
}
