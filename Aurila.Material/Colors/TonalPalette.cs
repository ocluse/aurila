namespace Aurila.Material.Colors;

/// <summary>
/// A single hue and chroma sampled at any tone from 0 (black) to 100 (white).
/// </summary>
/// <remarks>
/// Tones are cached because a scheme asks the same palette for the same handful of tones repeatedly,
/// and each miss runs the gamut solver.
/// </remarks>
public sealed class TonalPalette
{
    private readonly Dictionary<int, int> _cache = [];

    public double Hue { get; }

    public double Chroma { get; }

    private TonalPalette(double hue, double chroma)
    {
        Hue = hue;
        Chroma = chroma;
    }

    public static TonalPalette FromHueAndChroma(double hue, double chroma)
        => new(MathUtils.SanitizeDegrees(hue), chroma);

    /// <summary>Creates a palette passing through <paramref name="argb"/> at its own tone.</summary>
    public static TonalPalette FromInt(int argb)
    {
        Hct hct = Hct.FromInt(argb);
        return new TonalPalette(hct.Hue, hct.Chroma);
    }

    public int Tone(int tone)
    {
        if (_cache.TryGetValue(tone, out int cached))
        {
            return cached;
        }

        int argb = HctSolver.SolveToInt(Hue, Chroma, tone);
        _cache[tone] = argb;
        return argb;
    }

    public string Hex(int tone) => ColorUtils.HexFromArgb(Tone(tone));
}
