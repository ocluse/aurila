namespace Aurila.Material.Colors;

/// <summary>
/// The complete set of Material 3 colour roles, as CSS colour strings.
/// </summary>
/// <remarks>
/// <para>
/// Every property is <c>init</c>-only on a record, so overriding a single role is one expression and
/// leaves the rest of the generated scheme intact:
/// </para>
/// <code>
/// scheme = scheme with { Primary = "#FF6D00", OnPrimary = "#FFFFFF" };
/// </code>
/// <para>
/// Roles are named after the specification rather than after the controls that use them, so values
/// copied out of the Material 3 documentation or a Figma export map across without translation.
/// </para>
/// </remarks>
public record ColorScheme
{
    public required string Primary { get; init; }
    public required string OnPrimary { get; init; }
    public required string PrimaryContainer { get; init; }
    public required string OnPrimaryContainer { get; init; }

    public required string Secondary { get; init; }
    public required string OnSecondary { get; init; }
    public required string SecondaryContainer { get; init; }
    public required string OnSecondaryContainer { get; init; }

    public required string Tertiary { get; init; }
    public required string OnTertiary { get; init; }
    public required string TertiaryContainer { get; init; }
    public required string OnTertiaryContainer { get; init; }

    public required string Error { get; init; }
    public required string OnError { get; init; }
    public required string ErrorContainer { get; init; }
    public required string OnErrorContainer { get; init; }

    public required string Background { get; init; }
    public required string OnBackground { get; init; }

    public required string Surface { get; init; }
    public required string OnSurface { get; init; }
    public required string SurfaceVariant { get; init; }
    public required string OnSurfaceVariant { get; init; }
    public required string SurfaceDim { get; init; }
    public required string SurfaceBright { get; init; }
    public required string SurfaceContainerLowest { get; init; }
    public required string SurfaceContainerLow { get; init; }
    public required string SurfaceContainer { get; init; }
    public required string SurfaceContainerHigh { get; init; }
    public required string SurfaceContainerHighest { get; init; }
    public required string SurfaceTint { get; init; }

    public required string Outline { get; init; }
    public required string OutlineVariant { get; init; }

    public required string Shadow { get; init; }
    public required string Scrim { get; init; }

    public required string InverseSurface { get; init; }
    public required string InverseOnSurface { get; init; }
    public required string InversePrimary { get; init; }

    public required string PrimaryFixed { get; init; }
    public required string PrimaryFixedDim { get; init; }
    public required string OnPrimaryFixed { get; init; }
    public required string OnPrimaryFixedVariant { get; init; }

    public required string SecondaryFixed { get; init; }
    public required string SecondaryFixedDim { get; init; }
    public required string OnSecondaryFixed { get; init; }
    public required string OnSecondaryFixedVariant { get; init; }

    public required string TertiaryFixed { get; init; }
    public required string TertiaryFixedDim { get; init; }
    public required string OnTertiaryFixed { get; init; }
    public required string OnTertiaryFixedVariant { get; init; }

    /// <summary>Builds the light scheme for a set of palettes.</summary>
    public static ColorScheme Light(MaterialPalettes p)
    {
        ColorScheme scheme = StandardLight(p);

        if (p.Variant != SchemeVariant.Monochrome)
        {
            return scheme;
        }

        // Monochrome has no hue to carry meaning, so the specification pushes its accent roles to
        // near-extreme tones to keep them separable by lightness alone.
        return scheme with
        {
            Primary = p.Primary.Hex(0),
            OnPrimary = p.Primary.Hex(90),
            PrimaryContainer = p.Primary.Hex(25),
            OnPrimaryContainer = p.Primary.Hex(100),

            OnSecondaryContainer = p.Secondary.Hex(10),
            SecondaryContainer = p.Secondary.Hex(85),

            Tertiary = p.Tertiary.Hex(25),
            OnTertiary = p.Tertiary.Hex(90),
            TertiaryContainer = p.Tertiary.Hex(49),
            OnTertiaryContainer = p.Tertiary.Hex(100),

            OnErrorContainer = p.Error.Hex(10),

            PrimaryFixed = p.Primary.Hex(40),
            PrimaryFixedDim = p.Primary.Hex(30),
            OnPrimaryFixed = p.Primary.Hex(100),
            OnPrimaryFixedVariant = p.Primary.Hex(90),

            SecondaryFixed = p.Secondary.Hex(80),
            SecondaryFixedDim = p.Secondary.Hex(70),
            OnSecondaryFixed = p.Secondary.Hex(10),
            OnSecondaryFixedVariant = p.Secondary.Hex(25),

            TertiaryFixed = p.Tertiary.Hex(40),
            TertiaryFixedDim = p.Tertiary.Hex(30),
            OnTertiaryFixed = p.Tertiary.Hex(100),
            OnTertiaryFixedVariant = p.Tertiary.Hex(90),
        };
    }

    /// <summary>Builds the dark scheme for a set of palettes.</summary>
    /// <remarks>
    /// The fixed roles are deliberately identical to the light scheme's — that is what "fixed" means:
    /// they hold still across a theme change so that shared surfaces do not flip.
    /// </remarks>
    public static ColorScheme Dark(MaterialPalettes p)
    {
        ColorScheme scheme = StandardDark(p);

        if (p.Variant != SchemeVariant.Monochrome)
        {
            return scheme;
        }

        return scheme with
        {
            Primary = p.Primary.Hex(100),
            OnPrimary = p.Primary.Hex(10),
            PrimaryContainer = p.Primary.Hex(85),
            OnPrimaryContainer = p.Primary.Hex(0),

            OnSecondary = p.Secondary.Hex(10),

            Tertiary = p.Tertiary.Hex(90),
            OnTertiary = p.Tertiary.Hex(10),
            TertiaryContainer = p.Tertiary.Hex(60),
            OnTertiaryContainer = p.Tertiary.Hex(0),

            PrimaryFixed = p.Primary.Hex(40),
            PrimaryFixedDim = p.Primary.Hex(30),
            OnPrimaryFixed = p.Primary.Hex(100),
            OnPrimaryFixedVariant = p.Primary.Hex(90),

            SecondaryFixed = p.Secondary.Hex(80),
            SecondaryFixedDim = p.Secondary.Hex(70),
            OnSecondaryFixed = p.Secondary.Hex(10),
            OnSecondaryFixedVariant = p.Secondary.Hex(25),

            TertiaryFixed = p.Tertiary.Hex(40),
            TertiaryFixedDim = p.Tertiary.Hex(30),
            OnTertiaryFixed = p.Tertiary.Hex(100),
            OnTertiaryFixedVariant = p.Tertiary.Hex(90),
        };
    }

    private static ColorScheme StandardLight(MaterialPalettes p) => new()
    {
        Primary = p.Primary.Hex(40),
        OnPrimary = p.Primary.Hex(100),
        PrimaryContainer = p.Primary.Hex(90),
        OnPrimaryContainer = p.Primary.Hex(30),

        Secondary = p.Secondary.Hex(40),
        OnSecondary = p.Secondary.Hex(100),
        SecondaryContainer = p.Secondary.Hex(90),
        OnSecondaryContainer = p.Secondary.Hex(30),

        Tertiary = p.Tertiary.Hex(40),
        OnTertiary = p.Tertiary.Hex(100),
        TertiaryContainer = p.Tertiary.Hex(90),
        OnTertiaryContainer = p.Tertiary.Hex(30),

        Error = p.Error.Hex(40),
        OnError = p.Error.Hex(100),
        ErrorContainer = p.Error.Hex(90),
        OnErrorContainer = p.Error.Hex(30),

        Background = p.Neutral.Hex(98),
        OnBackground = p.Neutral.Hex(10),

        Surface = p.Neutral.Hex(98),
        OnSurface = p.Neutral.Hex(10),
        SurfaceVariant = p.NeutralVariant.Hex(90),
        OnSurfaceVariant = p.NeutralVariant.Hex(30),
        SurfaceDim = p.Neutral.Hex(87),
        SurfaceBright = p.Neutral.Hex(98),
        SurfaceContainerLowest = p.Neutral.Hex(100),
        SurfaceContainerLow = p.Neutral.Hex(96),
        SurfaceContainer = p.Neutral.Hex(94),
        SurfaceContainerHigh = p.Neutral.Hex(92),
        SurfaceContainerHighest = p.Neutral.Hex(90),
        SurfaceTint = p.Primary.Hex(40),

        Outline = p.NeutralVariant.Hex(50),
        OutlineVariant = p.NeutralVariant.Hex(80),

        Shadow = p.Neutral.Hex(0),
        Scrim = p.Neutral.Hex(0),

        InverseSurface = p.Neutral.Hex(20),
        InverseOnSurface = p.Neutral.Hex(95),
        InversePrimary = p.Primary.Hex(80),

        PrimaryFixed = p.Primary.Hex(90),
        PrimaryFixedDim = p.Primary.Hex(80),
        OnPrimaryFixed = p.Primary.Hex(10),
        OnPrimaryFixedVariant = p.Primary.Hex(30),

        SecondaryFixed = p.Secondary.Hex(90),
        SecondaryFixedDim = p.Secondary.Hex(80),
        OnSecondaryFixed = p.Secondary.Hex(10),
        OnSecondaryFixedVariant = p.Secondary.Hex(30),

        TertiaryFixed = p.Tertiary.Hex(90),
        TertiaryFixedDim = p.Tertiary.Hex(80),
        OnTertiaryFixed = p.Tertiary.Hex(10),
        OnTertiaryFixedVariant = p.Tertiary.Hex(30),
    };

    private static ColorScheme StandardDark(MaterialPalettes p) => new()
    {
        Primary = p.Primary.Hex(80),
        OnPrimary = p.Primary.Hex(20),
        PrimaryContainer = p.Primary.Hex(30),
        OnPrimaryContainer = p.Primary.Hex(90),

        Secondary = p.Secondary.Hex(80),
        OnSecondary = p.Secondary.Hex(20),
        SecondaryContainer = p.Secondary.Hex(30),
        OnSecondaryContainer = p.Secondary.Hex(90),

        Tertiary = p.Tertiary.Hex(80),
        OnTertiary = p.Tertiary.Hex(20),
        TertiaryContainer = p.Tertiary.Hex(30),
        OnTertiaryContainer = p.Tertiary.Hex(90),

        Error = p.Error.Hex(80),
        OnError = p.Error.Hex(20),
        ErrorContainer = p.Error.Hex(30),
        OnErrorContainer = p.Error.Hex(90),

        Background = p.Neutral.Hex(6),
        OnBackground = p.Neutral.Hex(90),

        Surface = p.Neutral.Hex(6),
        OnSurface = p.Neutral.Hex(90),
        SurfaceVariant = p.NeutralVariant.Hex(30),
        OnSurfaceVariant = p.NeutralVariant.Hex(80),
        SurfaceDim = p.Neutral.Hex(6),
        SurfaceBright = p.Neutral.Hex(24),
        SurfaceContainerLowest = p.Neutral.Hex(4),
        SurfaceContainerLow = p.Neutral.Hex(10),
        SurfaceContainer = p.Neutral.Hex(12),
        SurfaceContainerHigh = p.Neutral.Hex(17),
        SurfaceContainerHighest = p.Neutral.Hex(22),
        SurfaceTint = p.Primary.Hex(80),

        Outline = p.NeutralVariant.Hex(60),
        OutlineVariant = p.NeutralVariant.Hex(30),

        Shadow = p.Neutral.Hex(0),
        Scrim = p.Neutral.Hex(0),

        InverseSurface = p.Neutral.Hex(90),
        InverseOnSurface = p.Neutral.Hex(20),
        InversePrimary = p.Primary.Hex(40),

        PrimaryFixed = p.Primary.Hex(90),
        PrimaryFixedDim = p.Primary.Hex(80),
        OnPrimaryFixed = p.Primary.Hex(10),
        OnPrimaryFixedVariant = p.Primary.Hex(30),

        SecondaryFixed = p.Secondary.Hex(90),
        SecondaryFixedDim = p.Secondary.Hex(80),
        OnSecondaryFixed = p.Secondary.Hex(10),
        OnSecondaryFixedVariant = p.Secondary.Hex(30),

        TertiaryFixed = p.Tertiary.Hex(90),
        TertiaryFixedDim = p.Tertiary.Hex(80),
        OnTertiaryFixed = p.Tertiary.Hex(10),
        OnTertiaryFixedVariant = p.Tertiary.Hex(30),
    };

    /// <summary>Enumerates every role as a (name, value) pair, in declaration order.</summary>
    public IEnumerable<KeyValuePair<string, string>> EnumerateRoles()
    {
        foreach (var property in typeof(ColorScheme).GetProperties())
        {
            if (property.PropertyType == typeof(string) && property.GetValue(this) is string value)
            {
                yield return new KeyValuePair<string, string>(property.Name, value);
            }
        }
    }
}
