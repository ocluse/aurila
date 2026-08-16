namespace Aurila.Material.Colors;

/// <summary>
/// A colour expressed as hue, chroma and tone.
/// </summary>
/// <remarks>
/// Tone is CIE L*, so two colours with the same tone have the same measured lightness regardless of
/// hue. That is the property the whole Material 3 colour system rests on: a role assigned tone 40
/// contrasts with a role assigned tone 100 by the same amount for every seed colour.
/// </remarks>
public readonly struct Hct
{
    public double Hue { get; }

    public double Chroma { get; }

    public double Tone { get; }

    public Hct(double hue, double chroma, double tone)
    {
        Hue = hue;
        Chroma = chroma;
        Tone = tone;
    }

    public static Hct FromInt(int argb)
    {
        Cam16 cam = Cam16.FromInt(argb);
        return new Hct(cam.Hue, cam.Chroma, ColorUtils.LstarFromArgb(argb));
    }

    public static Hct FromHex(string hex) => FromInt(ColorUtils.ArgbFromHex(hex));

    /// <summary>
    /// The nearest sRGB colour to this specification, gamut-mapped when it does not exist exactly.
    /// </summary>
    public int ToInt() => HctSolver.SolveToInt(Hue, Chroma, Tone);

    public string ToHex() => ColorUtils.HexFromArgb(ToInt());

    public override string ToString()
        => $"HCT({Hue:F1}, {Chroma:F1}, {Tone:F1}) {ToHex()}";
}
