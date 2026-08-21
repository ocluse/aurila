using Aurila.Components;
using Aurila.Components.Controls;
using Aurila.Components.Input;
using Aurila.Components.Layout;
using Aurila.Components.Modals;

namespace Aurila.Fluent.Appearance;

public static class FluentButton
{
    public static IAppearance<AuButton> Primary { get; } = Make("primary");
    public static IAppearance<AuButton> Secondary { get; } = Make("secondary");
    public static IAppearance<AuButton> Outline { get; } = Make("outline");
    public static IAppearance<AuButton> Subtle { get; } = Make("subtle");
    public static IAppearance<AuButton> Transparent { get; } = Make("transparent");
    private static ClassAppearance<AuButton> Make(string variant) => new("fl-button", $"fl-button--{variant}");
}

public static class FluentIconButton
{
    public static IAppearance<AuIconButton> Secondary { get; } = Make("secondary");
    public static IAppearance<AuIconButton> Primary { get; } = Make("primary");
    public static IAppearance<AuIconButton> Outline { get; } = Make("outline");
    public static IAppearance<AuIconButton> Subtle { get; } = Make("subtle");
    public static IAppearance<AuIconButton> Transparent { get; } = Make("transparent");
    private static ClassAppearance<AuIconButton> Make(string variant) => new("fl-icon-button", $"fl-button--{variant}");
}

public static class FluentFab
{
    public static IAppearance<AuFloatingActionButton> Brand { get; } = Make("brand", null);
    public static IAppearance<AuFloatingActionButton> Neutral { get; } = Make("neutral", null);
    public static IAppearance<AuFloatingActionButton> BrandSmall { get; } = Make("brand", "small");
    public static IAppearance<AuFloatingActionButton> BrandLarge { get; } = Make("brand", "large");
    private static ClassAppearance<AuFloatingActionButton> Make(string color, string? size)
        => size is null ? new("fl-fab", $"fl-fab--{color}") : new("fl-fab", $"fl-fab--{color}", $"fl-fab--{size}");
}

public static class FluentCard
{
    public static IAppearance<AuCard> Filled { get; } = Make("filled");
    public static IAppearance<AuCard> FilledAlternative { get; } = Make("filled-alternative");
    public static IAppearance<AuCard> Subtle { get; } = Make("subtle");
    public static IAppearance<AuCard> Outline { get; } = Make("outline");
    [Obsolete("Fluent 2 calls the elevated surface the Filled card appearance. Use Filled instead.")]
    public static IAppearance<AuCard> Elevated => Filled;
    private static ClassAppearance<AuCard> Make(string variant) => new("fl-card", $"fl-card--{variant}");
}

/// <summary>Fluent 2 tag appearances, applied to Aurila's chip primitive.</summary>
public static class FluentTag
{
    public static IAppearance<AuChip> Filled { get; } = Make("filled");
    public static IAppearance<AuChip> Brand { get; } = Make("brand");
    public static IAppearance<AuChip> Outline { get; } = Make("outline");
    private static ClassAppearance<AuChip> Make(string variant) => new("fl-tag", $"fl-tag--{variant}");
}

/// <summary>Compatibility aliases for applications that previously used the Material-oriented chip name.</summary>
public static class FluentChip
{
    public static IAppearance<AuChip> Neutral => FluentTag.Filled;
    public static IAppearance<AuChip> Brand => FluentTag.Brand;
    public static IAppearance<AuChip> Outline => FluentTag.Outline;
}

public static class FluentTopAppBar
{
    public static IAppearance<AuTopAppBar> Neutral { get; } = Make("neutral");
    public static IAppearance<AuTopAppBar> Brand { get; } = Make("brand");
    public static IAppearance<AuTopAppBar> Transparent { get; } = Make("transparent");
    private static ClassAppearance<AuTopAppBar> Make(string variant) => new("fl-top-app-bar", $"fl-top-app-bar--{variant}");
}

public static class FluentDialog
{
    public static IAppearance<AuDialog> Standard { get; } = new ClassAppearance<AuDialog>("fl-dialog");
}

public static class FluentBottomSheet
{
    public static IAppearance<AuBottomSheet> Standard { get; } = new ClassAppearance<AuBottomSheet>("fl-bottom-sheet");
}

public static class FluentField
{
    public static IAppearance<AuTextField> Outline { get; } = OutlineFor<AuTextField>();
    public static IAppearance<AuTextField> FilledDarker { get; } = FilledDarkerFor<AuTextField>();
    public static IAppearance<AuTextField> FilledLighter { get; } = FilledLighterFor<AuTextField>();
    public static IAppearance<AuTextField> Underline { get; } = UnderlineFor<AuTextField>();
    public static IAppearance<AuTextBox> OutlineBox { get; } = OutlineFor<AuTextBox>();
    public static IAppearance<AuTextBox> FilledDarkerBox { get; } = FilledDarkerFor<AuTextBox>();
    public static IAppearance<AuTextBox> FilledLighterBox { get; } = FilledLighterFor<AuTextBox>();
    public static IAppearance<AuTextBox> UnderlineBox { get; } = UnderlineFor<AuTextBox>();
    public static IAppearance<T> OutlineFor<T>() where T : AuControlBase<T> => Make<T>("outline");
    public static IAppearance<T> FilledDarkerFor<T>() where T : AuControlBase<T> => Make<T>("filled-darker");
    public static IAppearance<T> FilledLighterFor<T>() where T : AuControlBase<T> => Make<T>("filled-lighter");
    public static IAppearance<T> UnderlineFor<T>() where T : AuControlBase<T> => Make<T>("underline");
    [Obsolete("Use Outline.")]
    public static IAppearance<AuTextField> Outlined => Outline;
    [Obsolete("Use FilledDarker.")]
    public static IAppearance<AuTextField> Filled => FilledDarker;
    [Obsolete("Use OutlineBox.")]
    public static IAppearance<AuTextBox> OutlinedBox => OutlineBox;
    [Obsolete("Use FilledDarkerBox.")]
    public static IAppearance<AuTextBox> FilledBox => FilledDarkerBox;
    public static IAppearance<T> OutlinedFor<T>() where T : AuControlBase<T> => OutlineFor<T>();
    public static IAppearance<T> FilledFor<T>() where T : AuControlBase<T> => FilledDarkerFor<T>();
    private static ClassAppearance<T> Make<T>(string appearance) where T : AuControlBase<T> => new("fl-field", $"fl-field--{appearance}");
}

public static class FluentSpinner
{
    public static IAppearance<AuCircularProgress> Default { get; } = new ClassAppearance<AuCircularProgress>("fl-spinner");
}

public static class FluentSurface
{
    public static IAppearance<AuSurface> Level0 { get; } = Make(0);
    public static IAppearance<AuSurface> Level1 { get; } = Make(1);
    public static IAppearance<AuSurface> Level2 { get; } = Make(2);
    public static IAppearance<AuSurface> Level3 { get; } = Make(3);
    public static IAppearance<AuSurface> Level4 { get; } = Make(4);
    private static ClassAppearance<AuSurface> Make(int level) => new("fl-surface", $"fl-surface--level{level}");
}
