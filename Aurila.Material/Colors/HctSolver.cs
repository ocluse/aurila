namespace Aurila.Material.Colors;

/// <summary>
/// Finds the sRGB colour with a requested hue, chroma and tone.
/// </summary>
/// <remarks>
/// <para>
/// Most requested colours do not exist in sRGB — chroma 48 at tone 90 is outside the gamut for most
/// hues. The solver first tries for an exact answer by Newton iteration on CAM16 lightness, and when
/// that fails walks the gamut boundary to the most chromatic colour that keeps the requested hue and
/// tone. That two-step shape is what makes tonal palettes behave predictably at their extremes.
/// </para>
/// <para>
/// The transform matrices are derived from <see cref="ViewingConditions.Default"/> at type
/// initialisation rather than hardcoded, so they stay consistent with the model above them.
/// </para>
/// </remarks>
internal static class HctSolver
{
    private static readonly double[][] ScaledDiscountFromLinrgb;
    private static readonly double[][] LinrgbFromScaledDiscount;
    private static readonly double[] YFromLinrgb = [0.2126, 0.7152, 0.0722];
    private static readonly double[] CriticalPlanes = new double[255];

    static HctSolver()
    {
        ViewingConditions vc = ViewingConditions.Default;

        // linear RGB -> cone response -> discounted and scaled by the luminance adaptation factor.
        double[][] coneFromLinrgb = MathUtils.MatrixMultiply(ColorUtils.Cat16, ColorUtils.SrgbToXyz);

        ScaledDiscountFromLinrgb = new double[3][];
        for (int i = 0; i < 3; i++)
        {
            double scale = vc.RgbD[i] * vc.Fl / 100.0;
            ScaledDiscountFromLinrgb[i] =
            [
                scale * coneFromLinrgb[i][0],
                scale * coneFromLinrgb[i][1],
                scale * coneFromLinrgb[i][2],
            ];
        }

        LinrgbFromScaledDiscount = MathUtils.Invert(ScaledDiscountFromLinrgb);

        // The linear-RGB values at which the 8-bit quantisation boundaries fall.
        for (int i = 0; i < 255; i++)
        {
            CriticalPlanes[i] = ColorUtils.Linearized(i + 0.5);
        }
    }

    public static int SolveToInt(double hueDegrees, double chroma, double lstar)
    {
        if (chroma < 0.0001 || lstar < 0.0001 || lstar > 99.9999)
        {
            return ColorUtils.ArgbFromLstar(lstar);
        }

        hueDegrees = MathUtils.SanitizeDegrees(hueDegrees);
        double hueRadians = hueDegrees / 180.0 * Math.PI;
        double y = ColorUtils.YFromLstar(lstar);

        int exactAnswer = FindResultByJ(hueRadians, chroma, y);
        if (exactAnswer != 0)
        {
            return exactAnswer;
        }

        return ColorUtils.ArgbFromLinrgb(BisectToLimit(y, hueRadians));
    }

    private static double ChromaticAdaptation(double component)
    {
        double af = Math.Pow(Math.Abs(component), 0.42);
        return MathUtils.Signum(component) * 400.0 * af / (af + 27.13);
    }

    private static double InverseChromaticAdaptation(double adapted)
    {
        double adaptedAbs = Math.Abs(adapted);
        double basis = Math.Max(0.0, 27.13 * adaptedAbs / (400.0 - adaptedAbs));
        return MathUtils.Signum(adapted) * Math.Pow(basis, 1.0 / 0.42);
    }

    private static double HueOf(double[] linrgb)
    {
        double[] scaledDiscount = MathUtils.MatrixMultiply(linrgb, ScaledDiscountFromLinrgb);
        double rA = ChromaticAdaptation(scaledDiscount[0]);
        double gA = ChromaticAdaptation(scaledDiscount[1]);
        double bA = ChromaticAdaptation(scaledDiscount[2]);

        double a = (11.0 * rA + -12.0 * gA + bA) / 11.0;
        double b = (rA + gA - 2.0 * bA) / 9.0;
        return Math.Atan2(b, a);
    }

    private static bool AreInCyclicOrder(double a, double b, double c)
        => MathUtils.SanitizeRadians(b - a) < MathUtils.SanitizeRadians(c - a);

    private static double[] LerpPoint(double[] source, double t, double[] target)
    =>
    [
        source[0] + (target[0] - source[0]) * t,
        source[1] + (target[1] - source[1]) * t,
        source[2] + (target[2] - source[2]) * t,
    ];

    private static double[] SetCoordinate(double[] source, double coordinate, double[] target, int axis)
    {
        double t = (coordinate - source[axis]) / (target[axis] - source[axis]);
        return LerpPoint(source, t, target);
    }

    private static bool IsBounded(double x) => x is >= 0.0 and <= 100.0;

    /// <summary>
    /// The n-th vertex of the polygon formed by intersecting the constant-Y plane with the RGB cube.
    /// </summary>
    private static double[] NthVertex(double y, int n)
    {
        double kR = YFromLinrgb[0], kG = YFromLinrgb[1], kB = YFromLinrgb[2];
        double coordA = n % 4 <= 1 ? 0.0 : 100.0;
        double coordB = n % 2 == 0 ? 0.0 : 100.0;

        if (n < 4)
        {
            double g = coordA, b = coordB;
            double r = (y - g * kG - b * kB) / kR;
            return IsBounded(r) ? [r, g, b] : [-1.0, -1.0, -1.0];
        }

        if (n < 8)
        {
            double b = coordA, r = coordB;
            double g = (y - r * kR - b * kB) / kG;
            return IsBounded(g) ? [r, g, b] : [-1.0, -1.0, -1.0];
        }
        else
        {
            double r = coordA, g = coordB;
            double b = (y - r * kR - g * kG) / kB;
            return IsBounded(b) ? [r, g, b] : [-1.0, -1.0, -1.0];
        }
    }

    private static (double[] Left, double[] Right) BisectToSegment(double y, double targetHue)
    {
        double[] left = [-1.0, -1.0, -1.0];
        double[] right = left;
        double leftHue = 0.0;
        double rightHue = 0.0;
        bool initialized = false;
        bool uncut = true;

        for (int n = 0; n < 12; n++)
        {
            double[] mid = NthVertex(y, n);
            if (mid[0] < 0)
            {
                continue;
            }

            double midHue = HueOf(mid);

            if (!initialized)
            {
                left = mid;
                right = mid;
                leftHue = midHue;
                rightHue = midHue;
                initialized = true;
                continue;
            }

            if (uncut || AreInCyclicOrder(leftHue, midHue, rightHue))
            {
                uncut = false;
                if (AreInCyclicOrder(leftHue, targetHue, midHue))
                {
                    right = mid;
                    rightHue = midHue;
                }
                else
                {
                    left = mid;
                    leftHue = midHue;
                }
            }
        }

        return (left, right);
    }

    private static double[] BisectToLimit(double y, double targetHue)
    {
        (double[] left, double[] right) = BisectToSegment(y, targetHue);
        double leftHue = HueOf(left);

        for (int axis = 0; axis < 3; axis++)
        {
            if (left[axis] == right[axis])
            {
                continue;
            }

            int lPlane;
            int rPlane;

            if (left[axis] < right[axis])
            {
                lPlane = (int)Math.Floor(ColorUtils.TrueDelinearized(left[axis]) - 0.5);
                rPlane = (int)Math.Ceiling(ColorUtils.TrueDelinearized(right[axis]) - 0.5);
            }
            else
            {
                lPlane = (int)Math.Ceiling(ColorUtils.TrueDelinearized(left[axis]) - 0.5);
                rPlane = (int)Math.Floor(ColorUtils.TrueDelinearized(right[axis]) - 0.5);
            }

            for (int i = 0; i < 8; i++)
            {
                if (Math.Abs(rPlane - lPlane) <= 1)
                {
                    break;
                }

                int mPlane = (int)Math.Floor((lPlane + rPlane) / 2.0);
                double midPlaneCoordinate = CriticalPlanes[Math.Clamp(mPlane, 0, 254)];
                double[] mid = SetCoordinate(left, midPlaneCoordinate, right, axis);
                double midHue = HueOf(mid);

                if (AreInCyclicOrder(leftHue, targetHue, midHue))
                {
                    right = mid;
                    rPlane = mPlane;
                }
                else
                {
                    left = mid;
                    leftHue = midHue;
                    lPlane = mPlane;
                }
            }
        }

        return [(left[0] + right[0]) / 2.0, (left[1] + right[1]) / 2.0, (left[2] + right[2]) / 2.0];
    }

    /// <summary>
    /// Newton iteration on CAM16 lightness. Returns 0 when the requested colour is outside sRGB.
    /// </summary>
    private static int FindResultByJ(double hueRadians, double chroma, double y)
    {
        ViewingConditions vc = ViewingConditions.Default;

        double j = Math.Sqrt(y) * 11.0;
        double tInnerCoeff = 1.0 / Math.Pow(1.64 - Math.Pow(0.29, vc.N), 0.73);
        double eHue = 0.25 * (Math.Cos(hueRadians + 2.0) + 3.8);
        double p1 = eHue * (50000.0 / 13.0) * vc.Nc * vc.Ncb;
        double hSin = Math.Sin(hueRadians);
        double hCos = Math.Cos(hueRadians);

        for (int iterationRound = 0; iterationRound < 5; iterationRound++)
        {
            double jNormalized = j / 100.0;
            double alpha = chroma == 0.0 || j == 0.0 ? 0.0 : chroma / Math.Sqrt(jNormalized);
            double t = Math.Pow(alpha * tInnerCoeff, 1.0 / 0.9);
            double ac = vc.Aw * Math.Pow(jNormalized, 1.0 / vc.C / vc.Z);
            double p2 = ac / vc.Nbb;

            double gamma = 23.0 * (p2 + 0.305) * t / (23.0 * p1 + 11.0 * t * hCos + 108.0 * t * hSin);
            double a = gamma * hCos;
            double b = gamma * hSin;

            double rA = (460.0 * p2 + 451.0 * a + 288.0 * b) / 1403.0;
            double gA = (460.0 * p2 - 891.0 * a - 261.0 * b) / 1403.0;
            double bA = (460.0 * p2 - 220.0 * a - 6300.0 * b) / 1403.0;

            double[] linrgb = MathUtils.MatrixMultiply(
            [
                InverseChromaticAdaptation(rA),
                InverseChromaticAdaptation(gA),
                InverseChromaticAdaptation(bA),
            ], LinrgbFromScaledDiscount);

            if (linrgb[0] < 0 || linrgb[1] < 0 || linrgb[2] < 0)
            {
                return 0;
            }

            double fnj = YFromLinrgb[0] * linrgb[0] + YFromLinrgb[1] * linrgb[1] + YFromLinrgb[2] * linrgb[2];
            if (fnj <= 0)
            {
                return 0;
            }

            if (iterationRound == 4 || Math.Abs(fnj - y) < 0.002)
            {
                if (linrgb[0] > 100.01 || linrgb[1] > 100.01 || linrgb[2] > 100.01)
                {
                    return 0;
                }

                return ColorUtils.ArgbFromLinrgb(linrgb);
            }

            j -= (fnj - y) * j / (2.0 * fnj);
        }

        return 0;
    }
}
