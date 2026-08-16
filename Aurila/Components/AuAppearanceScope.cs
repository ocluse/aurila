using Aurila.Contracts.Design.Appearance;

namespace Aurila.Components;

/// <summary>
/// Supplies an <see cref="IAppearanceProvider"/> to every Aurila control beneath it.
/// </summary>
/// <remarks>
/// <para>
/// Appearance packages are expected to rely on this component rather than cascade a provider
/// themselves. Place a scope lower in the tree with an explicit <see cref="Provider"/> to restyle a
/// subtree.
/// </para>
/// </remarks>
public sealed class AuAppearanceScope : ComponentBase
{
    private IAppearanceProvider? _effectiveProvider;

    /// <summary>
    /// The provider to cascade. When <see langword="null"/>, the provider registered in the service
    /// container is used, and the scope is inert if none is registered.
    /// </summary>
    [Parameter]
    public IAppearanceProvider? Provider { get; set; }

    /// <summary>
    /// Whether the cascaded provider is treated as immutable. Defaults to <see langword="true"/>,
    /// which lets Blazor skip change notifications for every control in the subtree.
    /// </summary>
    /// <remarks>
    /// Set to <see langword="false"/> only when <see cref="Provider"/> is swapped at runtime. Themes
    /// that recolour by rewriting CSS custom properties do not need to.
    /// </remarks>
    [Parameter]
    public bool IsFixed { get; set; } = true;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    private IServiceProvider Services { get; set; } = null!;

    protected override void OnParametersSet()
    {
        _effectiveProvider = Provider ?? Services.GetService(typeof(IAppearanceProvider)) as IAppearanceProvider;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<IAppearanceProvider>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<>.Value), _effectiveProvider);
            builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), IsFixed);
            builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), ChildContent);
        }
        builder.CloseComponent();
    }
}
