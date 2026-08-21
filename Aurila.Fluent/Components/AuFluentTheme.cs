using Aurila.Components;
using Aurila.Fluent.Services;

namespace Aurila.Fluent.Components;

/// <summary>Publishes Fluent semantic tokens and supplies Fluent appearances to its descendants.</summary>
public sealed class AuFluentTheme : ComponentBase, IDisposable
{
    [Inject] private FluentThemeService ThemeService { get; set; } = null!;
    [Inject] private IAppearanceProvider AppearanceProvider { get; set; } = null!;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized() => ThemeService.Changed += OnThemeChanged;
    public void Dispose() => ThemeService.Changed -= OnThemeChanged;
    private void OnThemeChanged() => _ = InvokeAsync(StateHasChanged);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "style");
        builder.AddAttribute(1, "type", "text/css");
        builder.AddContent(2, new MarkupString(ThemeService.Theme.BuildCss(ThemeService.Mode)));
        builder.CloseElement();

        builder.OpenComponent<AuAppearanceScope>(3);
        builder.AddAttribute(4, nameof(AuAppearanceScope.Provider), AppearanceProvider);
        builder.AddAttribute(5, nameof(AuAppearanceScope.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}
