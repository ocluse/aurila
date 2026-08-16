namespace Aurila.Material.Colors;

/// <summary>
/// The CAM16 viewing conditions. Material 3 evaluates every colour under a single fixed set, exposed
/// as <see cref="Default"/>; the constructor is kept general so the model stays inspectable.
/// </summary>
internal sealed class ViewingConditions
{
    /// <summary>sRGB-like viewing conditions: D65 white point, mid-grey background, average surround.</summary>
    public static ViewingConditions Default { get; } = Make();

    public double N { get; }
    public double Aw { get; }
    public double Nbb { get; }
    public double Ncb { get; }
    public double C { get; }
    public double Nc { get; }
    public double[] RgbD { get; }
    public double Fl { get; }
    public double FlRoot { get; }
    public double Z { get; }

    private ViewingConditions(double n, double aw, double nbb, double ncb, double c, double nc,
        double[] rgbD, double fl, double flRoot, double z)
    {
        N = n;
        Aw = aw;
        Nbb = nbb;
        Ncb = ncb;
        C = c;
        Nc = nc;
        RgbD = rgbD;
        Fl = fl;
        FlRoot = flRoot;
        Z = z;
    }

    public static ViewingConditions Make(
        double[]? whitePoint = null,
        double adaptingLuminance = -1.0,
        double backgroundLstar = 50.0,
        double surround = 2.0,
        bool discountingIlluminant = false)
    {
        double[] wp = whitePoint ?? ColorUtils.WhitePointD65;

        if (adaptingLuminance < 0)
        {
            adaptingLuminance = 200.0 / Math.PI * ColorUtils.YFromLstar(50.0) / 100.0;
        }

        backgroundLstar = Math.Max(0.1, backgroundLstar);

        double rW = wp[0] * 0.401288 + wp[1] * 0.650173 + wp[2] * -0.051461;
        double gW = wp[0] * -0.250268 + wp[1] * 1.204414 + wp[2] * 0.045854;
        double bW = wp[0] * -0.002079 + wp[1] * 0.048952 + wp[2] * 0.953127;

        double f = 0.8 + surround / 10.0;
        double c = f >= 0.9
            ? 0.59 + (0.69 - 0.59) * ((f - 0.9) * 10.0)
            : 0.525 + (0.59 - 0.525) * ((f - 0.8) * 10.0);

        double d = discountingIlluminant
            ? 1.0
            : f * (1.0 - 1.0 / 3.6 * Math.Exp((-adaptingLuminance - 42.0) / 92.0));
        d = Math.Clamp(d, 0.0, 1.0);

        double[] rgbD =
        [
            d * (100.0 / rW) + 1.0 - d,
            d * (100.0 / gW) + 1.0 - d,
            d * (100.0 / bW) + 1.0 - d,
        ];

        double k = 1.0 / (5.0 * adaptingLuminance + 1.0);
        double k4 = k * k * k * k;
        double k4F = 1.0 - k4;
        double fl = k4 * adaptingLuminance + 0.1 * k4F * k4F * Math.Cbrt(5.0 * adaptingLuminance);

        double n = ColorUtils.YFromLstar(backgroundLstar) / wp[1];
        double z = 1.48 + Math.Sqrt(n);
        double nbb = 0.725 / Math.Pow(n, 0.2);

        double[] rgbAFactors =
        [
            Math.Pow(fl * rgbD[0] * rW / 100.0, 0.42),
            Math.Pow(fl * rgbD[1] * gW / 100.0, 0.42),
            Math.Pow(fl * rgbD[2] * bW / 100.0, 0.42),
        ];

        double[] rgbA =
        [
            400.0 * rgbAFactors[0] / (rgbAFactors[0] + 27.13),
            400.0 * rgbAFactors[1] / (rgbAFactors[1] + 27.13),
            400.0 * rgbAFactors[2] / (rgbAFactors[2] + 27.13),
        ];

        double aw = (40.0 * rgbA[0] + 20.0 * rgbA[1] + rgbA[2]) / 20.0 * nbb;

        return new ViewingConditions(n, aw, nbb, nbb, c, f, rgbD, fl, Math.Pow(fl, 0.25), z);
    }
}
