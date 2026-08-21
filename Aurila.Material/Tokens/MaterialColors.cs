namespace Aurila.Material.Tokens;

/// <summary>CSS references to the colour roles emitted by <c>AuMaterialTheme</c>.</summary>
public static class MaterialColors
{
    public const string Primary = "var(--md-sys-color-primary)";
    public const string OnPrimary = "var(--md-sys-color-on-primary)";
    public const string PrimaryContainer = "var(--md-sys-color-primary-container)";
    public const string OnPrimaryContainer = "var(--md-sys-color-on-primary-container)";

    public const string Secondary = "var(--md-sys-color-secondary)";
    public const string OnSecondary = "var(--md-sys-color-on-secondary)";
    public const string SecondaryContainer = "var(--md-sys-color-secondary-container)";
    public const string OnSecondaryContainer = "var(--md-sys-color-on-secondary-container)";

    public const string Tertiary = "var(--md-sys-color-tertiary)";
    public const string OnTertiary = "var(--md-sys-color-on-tertiary)";
    public const string TertiaryContainer = "var(--md-sys-color-tertiary-container)";
    public const string OnTertiaryContainer = "var(--md-sys-color-on-tertiary-container)";

    public const string Error = "var(--md-sys-color-error)";
    public const string OnError = "var(--md-sys-color-on-error)";
    public const string ErrorContainer = "var(--md-sys-color-error-container)";
    public const string OnErrorContainer = "var(--md-sys-color-on-error-container)";

    public const string Background = "var(--md-sys-color-background)";
    public const string OnBackground = "var(--md-sys-color-on-background)";

    public const string Surface = "var(--md-sys-color-surface)";
    public const string OnSurface = "var(--md-sys-color-on-surface)";
    public const string SurfaceVariant = "var(--md-sys-color-surface-variant)";
    public const string OnSurfaceVariant = "var(--md-sys-color-on-surface-variant)";
    public const string SurfaceDim = "var(--md-sys-color-surface-dim)";
    public const string SurfaceBright = "var(--md-sys-color-surface-bright)";
    public const string SurfaceContainerLowest = "var(--md-sys-color-surface-container-lowest)";
    public const string SurfaceContainerLow = "var(--md-sys-color-surface-container-low)";
    public const string SurfaceContainer = "var(--md-sys-color-surface-container)";
    public const string SurfaceContainerHigh = "var(--md-sys-color-surface-container-high)";
    public const string SurfaceContainerHighest = "var(--md-sys-color-surface-container-highest)";
    public const string SurfaceTint = "var(--md-sys-color-surface-tint)";

    public const string Outline = "var(--md-sys-color-outline)";
    public const string OutlineVariant = "var(--md-sys-color-outline-variant)";

    public const string Shadow = "var(--md-sys-color-shadow)";
    public const string Scrim = "var(--md-sys-color-scrim)";

    public const string InverseSurface = "var(--md-sys-color-inverse-surface)";
    public const string InverseOnSurface = "var(--md-sys-color-inverse-on-surface)";
    public const string InversePrimary = "var(--md-sys-color-inverse-primary)";

    public const string PrimaryFixed = "var(--md-sys-color-primary-fixed)";
    public const string PrimaryFixedDim = "var(--md-sys-color-primary-fixed-dim)";
    public const string OnPrimaryFixed = "var(--md-sys-color-on-primary-fixed)";
    public const string OnPrimaryFixedVariant = "var(--md-sys-color-on-primary-fixed-variant)";

    public const string SecondaryFixed = "var(--md-sys-color-secondary-fixed)";
    public const string SecondaryFixedDim = "var(--md-sys-color-secondary-fixed-dim)";
    public const string OnSecondaryFixed = "var(--md-sys-color-on-secondary-fixed)";
    public const string OnSecondaryFixedVariant = "var(--md-sys-color-on-secondary-fixed-variant)";

    public const string TertiaryFixed = "var(--md-sys-color-tertiary-fixed)";
    public const string TertiaryFixedDim = "var(--md-sys-color-tertiary-fixed-dim)";
    public const string OnTertiaryFixed = "var(--md-sys-color-on-tertiary-fixed)";
    public const string OnTertiaryFixedVariant = "var(--md-sys-color-on-tertiary-fixed-variant)";

    /// <summary>Maps a <see cref="Colors.ColorScheme"/> property name onto its CSS custom property.</summary>
    public static string CssVariableName(string roleName)
    {
        Span<char> buffer = stackalloc char[roleName.Length * 2 + 16];
        "--md-sys-color-".AsSpan().CopyTo(buffer);
        int length = "--md-sys-color-".Length;

        for (int i = 0; i < roleName.Length; i++)
        {
            char c = roleName[i];
            if (char.IsUpper(c) && i > 0)
            {
                buffer[length++] = '-';
            }
            buffer[length++] = char.ToLowerInvariant(c);
        }

        return new string(buffer[..length]);
    }
}
