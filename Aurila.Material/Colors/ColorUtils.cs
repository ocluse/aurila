using System.Globalization;

namespace Aurila.Material.Colors;

/// <summary>
/// Conversions between sRGB, CIE XYZ and L*, in the exact form used by the Material 3 colour system.
/// </summary>
internal static class ColorUtils
{
    public static readonly double[][] SrgbToXyz =
    [
        [0.41233895, 0.35762064, 0.18051042],
        [0.2126, 0.7152, 0.0722],
        [0.01932141, 0.11916382, 0.95034478],
    ];

    public static readonly double[] WhitePointD65 = [95.047, 100.0, 108.883];

    /// <summary>The CAT16 chromatic adaptation transform, mapping XYZ onto cone responses.</summary>
    public static readonly double[][] Cat16 =
    [
        [0.401288, 0.650173, -0.051461],
        [-0.250268, 1.204414, 0.045854],
        [-0.002079, 0.048952, 0.953127],
    ];

    public static int ArgbFromRgb(int red, int green, int blue)
        => unchecked((int)0xFF000000) | ((red & 255) << 16) | ((green & 255) << 8) | (blue & 255);

    public static int RedFromArgb(int argb) => (argb >> 16) & 255;

    public static int GreenFromArgb(int argb) => (argb >> 8) & 255;

    public static int BlueFromArgb(int argb) => argb & 255;

    /// <summary>Converts an 8-bit sRGB component to linear RGB on a 0-100 scale.</summary>
    public static double Linearized(double rgbComponent)
    {
        double normalized = rgbComponent / 255.0;
        return normalized <= 0.040449936
            ? normalized / 12.92 * 100.0
            : Math.Pow((normalized + 0.055) / 1.055, 2.4) * 100.0;
    }

    /// <summary>Converts a linear RGB component on a 0-100 scale to an 8-bit sRGB component.</summary>
    public static int Delinearized(double rgbComponent)
    {
        double normalized = rgbComponent / 100.0;
        double delinearized = normalized <= 0.0031308
            ? normalized * 12.92
            : 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;

        return Math.Clamp((int)Math.Round(delinearized * 255.0, MidpointRounding.AwayFromZero), 0, 255);
    }

    /// <summary>The unrounded, unclamped form of <see cref="Delinearized"/>, used for gamut boundaries.</summary>
    public static double TrueDelinearized(double rgbComponent)
    {
        double normalized = rgbComponent / 100.0;
        double delinearized = normalized <= 0.0031308
            ? normalized * 12.92
            : 1.055 * Math.Pow(normalized, 1.0 / 2.4) - 0.055;

        return delinearized * 255.0;
    }

    private static double LabF(double t)
    {
        const double e = 216.0 / 24389.0;
        const double kappa = 24389.0 / 27.0;
        return t > e ? Math.Cbrt(t) : (kappa * t + 16.0) / 116.0;
    }

    private static double LabInvf(double ft)
    {
        const double e = 216.0 / 24389.0;
        const double kappa = 24389.0 / 27.0;
        double ft3 = ft * ft * ft;
        return ft3 > e ? ft3 : (116.0 * ft - 16.0) / kappa;
    }

    public static double YFromLstar(double lstar) => 100.0 * LabInvf((lstar + 16.0) / 116.0);

    public static double LstarFromY(double y) => LabF(y / 100.0) * 116.0 - 16.0;

    public static int ArgbFromLinrgb(double[] linrgb)
        => ArgbFromRgb(Delinearized(linrgb[0]), Delinearized(linrgb[1]), Delinearized(linrgb[2]));

    public static int ArgbFromLstar(double lstar)
    {
        int component = Delinearized(YFromLstar(lstar));
        return ArgbFromRgb(component, component, component);
    }

    public static double[] XyzFromArgb(int argb)
    {
        double[] linrgb =
        [
            Linearized(RedFromArgb(argb)),
            Linearized(GreenFromArgb(argb)),
            Linearized(BlueFromArgb(argb)),
        ];
        return MathUtils.MatrixMultiply(linrgb, SrgbToXyz);
    }

    public static double LstarFromArgb(int argb) => LstarFromY(XyzFromArgb(argb)[1]);

    public static string HexFromArgb(int argb)
        => $"#{RedFromArgb(argb):X2}{GreenFromArgb(argb):X2}{BlueFromArgb(argb):X2}";

    /// <summary>
    /// Parses <c>#RGB</c>, <c>#RRGGBB</c> and <c>#AARRGGBB</c>, with or without the leading hash.
    /// </summary>
    /// <exception cref="FormatException">The value is not a recognised hexadecimal colour.</exception>
    public static int ArgbFromHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);

        ReadOnlySpan<char> span = hex.AsSpan().Trim();
        if (span.Length > 0 && span[0] == '#')
        {
            span = span[1..];
        }

        static int Parse(ReadOnlySpan<char> value)
            => int.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        switch (span.Length)
        {
            case 3:
                int r = Parse(span.Slice(0, 1)), g = Parse(span.Slice(1, 1)), b = Parse(span.Slice(2, 1));
                return ArgbFromRgb(r * 17, g * 17, b * 17);
            case 6:
                return ArgbFromRgb(Parse(span.Slice(0, 2)), Parse(span.Slice(2, 2)), Parse(span.Slice(4, 2)));
            case 8:
                return ArgbFromRgb(Parse(span.Slice(2, 2)), Parse(span.Slice(4, 2)), Parse(span.Slice(6, 2)));
            default:
                throw new FormatException($"'{hex}' is not a valid hexadecimal colour.");
        }
    }
}
