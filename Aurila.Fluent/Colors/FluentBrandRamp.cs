using System.Globalization;

namespace Aurila.Fluent.Colors;

/// <summary>A Fluent brand ramp from shade 10 (darkest) to 160 (lightest).</summary>
public sealed class FluentBrandRamp
{
    private static readonly int[] ShadeNames = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160];
    private static readonly double[] ShadeFactors = [.18, .28, .38, .50, .62, .74, .86, 1, .88, .76, .64, .52, .40, .28, .16, .06];
    private readonly IReadOnlyDictionary<int, string> _shades;

    private FluentBrandRamp(string seed, IReadOnlyDictionary<int, string> shades)
    {
        Seed = seed;
        _shades = shades;
    }

    public string Seed { get; }

    public string this[int shade] => _shades.TryGetValue(shade, out string? value)
        ? value
        : throw new ArgumentOutOfRangeException(nameof(shade), "Fluent brand shades run from 10 to 160 in steps of 10.");

    public IEnumerable<KeyValuePair<int, string>> Enumerate() => _shades;

    public static FluentBrandRamp FromSeed(string seed)
    {
        Rgb source = Rgb.Parse(seed);
        Dictionary<int, string> shades = new(ShadeNames.Length);

        for (int index = 0; index < ShadeNames.Length; index++)
        {
            int shade = ShadeNames[index];
            double factor = ShadeFactors[index];
            Rgb mixed = shade <= 80
                ? Rgb.Mix(new Rgb(0, 0, 0), source, factor)
                : Rgb.Mix(new Rgb(255, 255, 255), source, factor);
            shades[shade] = mixed.ToHex();
        }

        return new FluentBrandRamp(source.ToHex(), shades);
    }

    private readonly record struct Rgb(byte Red, byte Green, byte Blue)
    {
        public static Rgb Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A Fluent seed colour is required.", nameof(value));
            }

            string hex = value.Trim().TrimStart('#');
            if (hex.Length == 3)
            {
                hex = string.Concat(hex.Select(c => new string(c, 2)));
            }

            if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int color))
            {
                throw new ArgumentException($"'{value}' is not a valid RGB hex colour.", nameof(value));
            }

            return new Rgb((byte)(color >> 16), (byte)(color >> 8), (byte)color);
        }

        public static Rgb Mix(Rgb background, Rgb foreground, double foregroundAmount)
        {
            static byte Channel(byte a, byte b, double t)
                => (byte)Math.Clamp((int)Math.Round(a + ((b - a) * t)), 0, 255);

            return new Rgb(
                Channel(background.Red, foreground.Red, foregroundAmount),
                Channel(background.Green, foreground.Green, foregroundAmount),
                Channel(background.Blue, foreground.Blue, foregroundAmount));
        }

        public string ToHex() => $"#{Red:X2}{Green:X2}{Blue:X2}";
    }
}
