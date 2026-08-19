using Aurila.Components;
using Aurila.Components.Controls;
using Aurila.Components.Input;
using Aurila.Components.Layout;
using Aurila.Components.Modals;

namespace Aurila.Material.Appearance;

// Pass these to a control's Appearance parameter to pick a variant:
//   <AuButton Appearance="MaterialButton.Outlined">Cancel</AuButton>
// Every control carries exactly one variant class, so no :not() rules are needed in the stylesheet.

public static class MaterialButton
{
    public static IAppearance<AuButton> Filled { get; } = Make("filled");
    public static IAppearance<AuButton> Tonal { get; } = Make("tonal");
    public static IAppearance<AuButton> Elevated { get; } = Make("elevated");
    public static IAppearance<AuButton> Outlined { get; } = Make("outlined");
    public static IAppearance<AuButton> Text { get; } = Make("text");

    private static ClassAppearance<AuButton> Make(string variant)
        => new("md-button", $"md-button--{variant}");
}

public static class MaterialIconButton
{
    public static IAppearance<AuIconButton> Standard { get; } = Make("standard");
    public static IAppearance<AuIconButton> Filled { get; } = Make("filled");
    public static IAppearance<AuIconButton> Tonal { get; } = Make("tonal");
    public static IAppearance<AuIconButton> Outlined { get; } = Make("outlined");

    private static ClassAppearance<AuIconButton> Make(string variant)
        => new("md-icon-button", $"md-icon-button--{variant}");
}

public static class MaterialFab
{
    public static IAppearance<AuFloatingActionButton> Primary { get; } = Make("primary", null);
    public static IAppearance<AuFloatingActionButton> Secondary { get; } = Make("secondary", null);
    public static IAppearance<AuFloatingActionButton> Tertiary { get; } = Make("tertiary", null);
    public static IAppearance<AuFloatingActionButton> Surface { get; } = Make("surface", null);

    public static IAppearance<AuFloatingActionButton> PrimarySmall { get; } = Make("primary", "small");
    public static IAppearance<AuFloatingActionButton> PrimaryLarge { get; } = Make("primary", "large");

    private static ClassAppearance<AuFloatingActionButton> Make(string color, string? size)
        => size is null
            ? new("md-fab", $"md-fab--{color}")
            : new("md-fab", $"md-fab--{color}", $"md-fab--{size}");
}

public static class MaterialCard
{
    public static IAppearance<AuCard> Elevated { get; } = Make("elevated");
    public static IAppearance<AuCard> Filled { get; } = Make("filled");
    public static IAppearance<AuCard> Outlined { get; } = Make("outlined");

    private static ClassAppearance<AuCard> Make(string variant) => new("md-card", $"md-card--{variant}");
}

public static class MaterialChip
{
    public static IAppearance<AuChip> Assist { get; } = Make("assist");
    public static IAppearance<AuChip> Filter { get; } = Make("filter");
    public static IAppearance<AuChip> Input { get; } = Make("input");
    public static IAppearance<AuChip> Suggestion { get; } = Make("suggestion");

    private static ClassAppearance<AuChip> Make(string variant) => new("md-chip", $"md-chip--{variant}");
}

public static class MaterialTopAppBar
{
    public static IAppearance<AuTopAppBar> Small { get; } = Make("small");
    public static IAppearance<AuTopAppBar> CenterAligned { get; } = Make("center-aligned");
    public static IAppearance<AuTopAppBar> Medium { get; } = Make("medium");
    public static IAppearance<AuTopAppBar> Large { get; } = Make("large");

    private static ClassAppearance<AuTopAppBar> Make(string variant)
        => new("md-top-app-bar", $"md-top-app-bar--{variant}");
}

public static class MaterialDialog
{
    public static IAppearance<AuDialog> Basic { get; } = new ClassAppearance<AuDialog>("md-dialog");
}

public static class MaterialBottomSheet
{
    public static IAppearance<AuBottomSheet> Standard { get; } = new ClassAppearance<AuBottomSheet>("md-bottom-sheet");
}

/// <summary>
/// Text field variants. The generic overloads exist because the picker and dropdown controls are
/// themselves generic, so their closed types cannot be registered on the provider up front.
/// </summary>
public static class MaterialField
{
    public static IAppearance<AuTextField> Outlined { get; } = OutlinedFor<AuTextField>();
    public static IAppearance<AuTextField> Filled { get; } = FilledFor<AuTextField>();

    public static IAppearance<AuTextBox> OutlinedBox { get; } = OutlinedFor<AuTextBox>();
    public static IAppearance<AuTextBox> FilledBox { get; } = FilledFor<AuTextBox>();

    public static IAppearance<T> OutlinedFor<T>() where T : AuControlBase<T>
        => new ClassAppearance<T>("md-field", "md-field--outlined");

    public static IAppearance<T> FilledFor<T>() where T : AuControlBase<T>
        => new ClassAppearance<T>("md-field", "md-field--filled");
}

public static class MaterialSurface
{
    public static IAppearance<AuSurface> Level0 { get; } = Make(0);
    public static IAppearance<AuSurface> Level1 { get; } = Make(1);
    public static IAppearance<AuSurface> Level2 { get; } = Make(2);
    public static IAppearance<AuSurface> Level3 { get; } = Make(3);
    public static IAppearance<AuSurface> Level4 { get; } = Make(4);
    public static IAppearance<AuSurface> Level5 { get; } = Make(5);

    private static ClassAppearance<AuSurface> Make(int level)
        => new("md-surface", $"md-surface--level{level}");
}
