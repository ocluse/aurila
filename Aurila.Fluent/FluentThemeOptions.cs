namespace Aurila.Fluent;

public enum FluentThemeMode
{
    System,
    Light,
    Dark,
}

/// <summary>Configuration for a generated Fluent 2 theme.</summary>
public sealed class FluentThemeOptions
{
    /// <summary>The brand colour used as the centre of the 16-step Fluent brand ramp.</summary>
    public string Seed { get; set; } = "#0F6CBD";

    public FluentThemeMode Mode { get; set; } = FluentThemeMode.System;

    /// <summary>Overrides applied after the light scheme is generated.</summary>
    public Func<FluentColorScheme, FluentColorScheme>? Light { get; set; }

    /// <summary>Overrides applied after the dark scheme is generated.</summary>
    public Func<FluentColorScheme, FluentColorScheme>? Dark { get; set; }

    public string FontFamily { get; set; } = "'Segoe UI', system-ui, sans-serif";
}
