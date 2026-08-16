namespace Aurila.Material.Colors;

/// <summary>
/// How strongly a generated scheme expresses the seed colour.
/// </summary>
public enum SchemeVariant
{
    /// <summary>
    /// The Material 3 default. A muted take on the seed, with a tertiary accent 60 degrees away.
    /// </summary>
    TonalSpot,

    /// <summary>Almost greyscale — the seed shows only as a faint tint.</summary>
    Neutral,

    /// <summary>Maximum chroma on the primary, with accents rotated away from the seed hue.</summary>
    Vibrant,

    /// <summary>Deliberately unexpected: the primary is rotated far from the seed.</summary>
    Expressive,

    /// <summary>Greyscale. Useful for high-contrast and print-like presentations.</summary>
    Monochrome,
}
