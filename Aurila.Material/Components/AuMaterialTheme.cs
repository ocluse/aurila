using Aurila.Components;
using Aurila.Material.Services;
using Microsoft.JSInterop;

namespace Aurila.Material.Components;

/// <summary>
/// Publishes the Material colour roles as CSS custom properties and supplies the appearance provider
/// to everything below it. Place it once, directly inside <c>AurilaApp</c>.
/// </summary>
public sealed class AuMaterialTheme : ComponentBase, IDisposable
{
    [Inject]
    private MaterialThemeService ThemeService { get; set; } = null!;

    [Inject]
    private IAppearanceProvider AppearanceProvider { get; set; } = null!;

    [Inject]
    private MaterialJsInterop JsInterop { get; set; } = null!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized() => ThemeService.Changed += OnThemeChanged;

    public void Dispose() => ThemeService.Changed -= OnThemeChanged;

    private void OnThemeChanged() => _ = InvokeAsync(StateHasChanged);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !ThemeService.RippleEnabled)
        {
            return;
        }

        try
        {
            await JsInterop.EnableRippleAsync();
        }
        catch (JSDisconnectedException)
        {
            // Navigated away before the module loaded.
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "style");
        {
            builder.AddAttribute(1, "type", "text/css");
            builder.AddContent(2, new MarkupString(ThemeService.Theme.BuildCss(ThemeService.Mode)));
        }
        builder.CloseElement();

        builder.OpenComponent<AuAppearanceScope>(3);
        {
            builder.AddAttribute(4, nameof(AuAppearanceScope.Provider), AppearanceProvider);
            builder.AddAttribute(5, nameof(AuAppearanceScope.ChildContent), ChildContent);
        }
        builder.CloseComponent();
    }
}
