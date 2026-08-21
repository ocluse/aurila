namespace Aurila.Fluent.Tokens;

/// <summary>CSS references for generated Fluent semantic colour aliases.</summary>
public static class FluentColors
{
    public const string NeutralForeground1 = "var(--fluent-color-neutral-foreground-1)";
    public const string NeutralForeground1Hover = "var(--fluent-color-neutral-foreground-1-hover)";
    public const string NeutralForeground2 = "var(--fluent-color-neutral-foreground-2)";
    public const string NeutralForeground3 = "var(--fluent-color-neutral-foreground-3)";
    public const string NeutralBackground1 = "var(--fluent-color-neutral-background-1)";
    public const string NeutralBackground1Hover = "var(--fluent-color-neutral-background-1-hover)";
    public const string NeutralBackground1Pressed = "var(--fluent-color-neutral-background-1-pressed)";
    public const string NeutralBackground2 = "var(--fluent-color-neutral-background-2)";
    public const string NeutralBackground3 = "var(--fluent-color-neutral-background-3)";
    public const string SubtleBackgroundHover = "var(--fluent-color-subtle-background-hover)";
    public const string BrandBackground = "var(--fluent-color-brand-background)";
    public const string BrandBackground2 = "var(--fluent-color-brand-background-2)";
    public const string BrandForeground1 = "var(--fluent-color-brand-foreground-1)";
    public const string BrandForeground2 = "var(--fluent-color-brand-foreground-2)";
    public const string BrandStroke1 = "var(--fluent-color-brand-stroke-1)";
    public const string NeutralStroke1 = "var(--fluent-color-neutral-stroke-1)";
    public const string DangerForeground = "var(--fluent-color-status-danger-foreground-1)";
    public const string SuccessForeground = "var(--fluent-color-status-success-foreground-1)";

    public static string CssVariableName(string roleName)
    {
        Span<char> buffer = stackalloc char[roleName.Length * 2 + 16];
        "--fluent-color-".AsSpan().CopyTo(buffer);
        int length = "--fluent-color-".Length;

        for (int index = 0; index < roleName.Length; index++)
        {
            char value = roleName[index];
            bool startsWord = char.IsUpper(value);
            bool startsNumericSuffix = char.IsDigit(value) && index > 0 && !char.IsDigit(roleName[index - 1]);
            if (index > 0 && (startsWord || startsNumericSuffix)) buffer[length++] = '-';
            buffer[length++] = char.ToLowerInvariant(value);
        }

        return new string(buffer[..length]);
    }
}
