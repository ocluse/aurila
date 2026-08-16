namespace Aurila.Material.Colors;

/// <summary>
/// The six tonal palettes a Material 3 scheme is built from.
/// </summary>
/// <remarks>
/// Each <see cref="SchemeVariant"/> is nothing more than a different set of hue rotations and chroma
/// values over the same seed. The error palette is fixed by the specification and ignores the seed,
/// so that "something went wrong" reads the same in every app.
/// </remarks>
public sealed class MaterialPalettes
{
    private static readonly double[] VibrantHues = [0, 41, 61, 101, 131, 181, 251, 301, 360];
    private static readonly double[] VibrantSecondaryRotations = [18, 15, 10, 12, 15, 18, 15, 12, 12];
    private static readonly double[] VibrantTertiaryRotations = [35, 30, 20, 25, 30, 35, 30, 25, 25];

    private static readonly double[] ExpressiveHues = [0, 21, 51, 121, 151, 191, 271, 321, 360];
    private static readonly double[] ExpressiveSecondaryRotations = [45, 95, 45, 20, 45, 90, 45, 45, 120];
    private static readonly double[] ExpressiveTertiaryRotations = [120, 120, 20, 45, 20, 15, 20, 120, 120];

    public Hct Source { get; }

    public SchemeVariant Variant { get; }

    public TonalPalette Primary { get; }

    public TonalPalette Secondary { get; }

    public TonalPalette Tertiary { get; }

    public TonalPalette Neutral { get; }

    public TonalPalette NeutralVariant { get; }

    public TonalPalette Error { get; }

    private MaterialPalettes(Hct source, SchemeVariant variant, TonalPalette primary, TonalPalette secondary,
        TonalPalette tertiary, TonalPalette neutral, TonalPalette neutralVariant, TonalPalette error)
    {
        Source = source;
        Variant = variant;
        Primary = primary;
        Secondary = secondary;
        Tertiary = tertiary;
        Neutral = neutral;
        NeutralVariant = neutralVariant;
        Error = error;
    }

    public static MaterialPalettes FromSeed(string seedHex, SchemeVariant variant = SchemeVariant.TonalSpot)
        => FromSeed(Hct.FromHex(seedHex), variant);

    public static MaterialPalettes FromSeed(Hct source, SchemeVariant variant = SchemeVariant.TonalSpot)
    {
        double hue = source.Hue;
        TonalPalette error = TonalPalette.FromHueAndChroma(25.0, 84.0);

        return variant switch
        {
            SchemeVariant.TonalSpot => new MaterialPalettes(source, variant,
                TonalPalette.FromHueAndChroma(hue, 36.0),
                TonalPalette.FromHueAndChroma(hue, 16.0),
                TonalPalette.FromHueAndChroma(hue + 60.0, 24.0),
                TonalPalette.FromHueAndChroma(hue, 6.0),
                TonalPalette.FromHueAndChroma(hue, 8.0),
                error),

            SchemeVariant.Neutral => new MaterialPalettes(source, variant,
                TonalPalette.FromHueAndChroma(hue, 12.0),
                TonalPalette.FromHueAndChroma(hue, 8.0),
                TonalPalette.FromHueAndChroma(hue, 16.0),
                TonalPalette.FromHueAndChroma(hue, 2.0),
                TonalPalette.FromHueAndChroma(hue, 2.0),
                error),

            SchemeVariant.Vibrant => new MaterialPalettes(source, variant,
                TonalPalette.FromHueAndChroma(hue, 200.0),
                TonalPalette.FromHueAndChroma(RotatedHue(hue, VibrantHues, VibrantSecondaryRotations), 24.0),
                TonalPalette.FromHueAndChroma(RotatedHue(hue, VibrantHues, VibrantTertiaryRotations), 32.0),
                TonalPalette.FromHueAndChroma(hue, 10.0),
                TonalPalette.FromHueAndChroma(hue, 12.0),
                error),

            SchemeVariant.Expressive => new MaterialPalettes(source, variant,
                TonalPalette.FromHueAndChroma(hue + 240.0, 40.0),
                TonalPalette.FromHueAndChroma(RotatedHue(hue, ExpressiveHues, ExpressiveSecondaryRotations), 24.0),
                TonalPalette.FromHueAndChroma(RotatedHue(hue, ExpressiveHues, ExpressiveTertiaryRotations), 32.0),
                TonalPalette.FromHueAndChroma(hue + 15.0, 8.0),
                TonalPalette.FromHueAndChroma(hue + 15.0, 12.0),
                error),

            SchemeVariant.Monochrome => new MaterialPalettes(source, variant,
                TonalPalette.FromHueAndChroma(hue, 0.0),
                TonalPalette.FromHueAndChroma(hue, 0.0),
                TonalPalette.FromHueAndChroma(hue, 0.0),
                TonalPalette.FromHueAndChroma(hue, 0.0),
                TonalPalette.FromHueAndChroma(hue, 0.0),
                error),

            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown scheme variant."),
        };
    }

    /// <summary>
    /// Rotates a hue by an amount that depends on which band of the colour wheel it falls in, so that
    /// accents stay distinguishable from the primary across the whole wheel.
    /// </summary>
    private static double RotatedHue(double sourceHue, double[] hues, double[] rotations)
    {
        for (int i = 0; i <= hues.Length - 2; i++)
        {
            // The lower bound is inclusive so that an achromatic seed, whose hue reads as exactly 0,
            // still picks up the first band's rotation rather than falling through unrotated.
            if (hues[i] <= sourceHue && sourceHue < hues[i + 1])
            {
                return MathUtils.SanitizeDegrees(sourceHue + rotations[i]);
            }
        }

        return sourceHue;
    }
}
