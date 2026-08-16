namespace Aurila.Material.Colors;

/// <summary>
/// A colour in the CAM16 appearance model. Only the forward transform is needed: turning an sRGB
/// colour into the hue and chroma that <see cref="Hct"/> reasons about.
/// </summary>
internal sealed class Cam16
{
    public double Hue { get; }
    public double Chroma { get; }
    public double J { get; }
    public double Q { get; }
    public double M { get; }
    public double S { get; }

    private Cam16(double hue, double chroma, double j, double q, double m, double s)
    {
        Hue = hue;
        Chroma = chroma;
        J = j;
        Q = q;
        M = m;
        S = s;
    }

    public static Cam16 FromInt(int argb) => FromInt(argb, ViewingConditions.Default);

    public static Cam16 FromInt(int argb, ViewingConditions vc)
    {
        double[] xyz = ColorUtils.XyzFromArgb(argb);
        double x = xyz[0], y = xyz[1], z = xyz[2];

        double rC = 0.401288 * x + 0.650173 * y - 0.051461 * z;
        double gC = -0.250268 * x + 1.204414 * y + 0.045854 * z;
        double bC = -0.002079 * x + 0.048952 * y + 0.953127 * z;

        double rD = vc.RgbD[0] * rC;
        double gD = vc.RgbD[1] * gC;
        double bD = vc.RgbD[2] * bC;

        double rAF = Math.Pow(vc.Fl * Math.Abs(rD) / 100.0, 0.42);
        double gAF = Math.Pow(vc.Fl * Math.Abs(gD) / 100.0, 0.42);
        double bAF = Math.Pow(vc.Fl * Math.Abs(bD) / 100.0, 0.42);

        double rA = MathUtils.Signum(rD) * 400.0 * rAF / (rAF + 27.13);
        double gA = MathUtils.Signum(gD) * 400.0 * gAF / (gAF + 27.13);
        double bA = MathUtils.Signum(bD) * 400.0 * bAF / (bAF + 27.13);

        double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
        double b = (rA + gA - 2.0 * bA) / 9.0;
        double u = (20.0 * rA + 20.0 * gA + 21.0 * bA) / 20.0;
        double p2 = (40.0 * rA + 20.0 * gA + bA) / 20.0;

        double hue = MathUtils.SanitizeDegrees(Math.Atan2(b, a) * 180.0 / Math.PI);

        double ac = p2 * vc.Nbb;
        double j = 100.0 * Math.Pow(ac / vc.Aw, vc.C * vc.Z);
        double q = 4.0 / vc.C * Math.Sqrt(j / 100.0) * (vc.Aw + 4.0) * vc.FlRoot;

        double huePrime = hue < 20.14 ? hue + 360.0 : hue;
        double eHue = 0.25 * (Math.Cos(huePrime * Math.PI / 180.0 + 2.0) + 3.8);
        double p1 = 50000.0 / 13.0 * eHue * vc.Nc * vc.Ncb;
        double t = p1 * Math.Sqrt(a * a + b * b) / (u + 0.305);

        double alpha = Math.Pow(t, 0.9) * Math.Pow(1.64 - Math.Pow(0.29, vc.N), 0.73);
        double chroma = alpha * Math.Sqrt(j / 100.0);
        double m = chroma * vc.FlRoot;
        double s = 50.0 * Math.Sqrt(alpha * vc.C / (vc.Aw + 4.0));

        return new Cam16(hue, chroma, j, q, m, s);
    }
}
