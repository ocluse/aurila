namespace Aurila.Material.Colors;

internal static class MathUtils
{
    public static double Signum(double value) => value < 0 ? -1 : value == 0 ? 0 : 1;

    public static double SanitizeDegrees(double degrees)
    {
        degrees %= 360.0;
        return degrees < 0 ? degrees + 360.0 : degrees;
    }

    public static double SanitizeRadians(double angle) => (angle + Math.PI * 8) % (Math.PI * 2);

    public static double[] MatrixMultiply(double[] row, double[][] matrix)
    {
        return
        [
            row[0] * matrix[0][0] + row[1] * matrix[0][1] + row[2] * matrix[0][2],
            row[0] * matrix[1][0] + row[1] * matrix[1][1] + row[2] * matrix[1][2],
            row[0] * matrix[2][0] + row[1] * matrix[2][1] + row[2] * matrix[2][2],
        ];
    }

    public static double[][] MatrixMultiply(double[][] a, double[][] b)
    {
        var result = new double[3][];
        for (int i = 0; i < 3; i++)
        {
            result[i] = new double[3];
            for (int j = 0; j < 3; j++)
            {
                result[i][j] = a[i][0] * b[0][j] + a[i][1] * b[1][j] + a[i][2] * b[2][j];
            }
        }
        return result;
    }

    public static double[][] Invert(double[][] m)
    {
        double a = m[0][0], b = m[0][1], c = m[0][2];
        double d = m[1][0], e = m[1][1], f = m[1][2];
        double g = m[2][0], h = m[2][1], i = m[2][2];

        double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);

        return
        [
            [(e * i - f * h) / det, (c * h - b * i) / det, (b * f - c * e) / det],
            [(f * g - d * i) / det, (a * i - c * g) / det, (c * d - a * f) / det],
            [(d * h - e * g) / det, (b * g - a * h) / det, (a * e - b * d) / det],
        ];
    }
}
