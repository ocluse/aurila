using Aurila.Components.Controls.Internal;
using Aurila.Contracts.Components;
using Aurila.Contracts.Navigation;

namespace Aurila.Components.Controls;

public sealed class AdaptiveNavigation : ControlBase<AdaptiveNavigation>, IAdaptiveNavigationHost, IDisposable
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public Type? CurrentPageType { get; set; }

    [Parameter]
    public string? CurrentRoute { get; set; }

    [Parameter]
    public ActiveMatch DefaultMatchMode { get; set; } = ActiveMatch.Prefix;

    [Parameter]
    public Func<AdaptiveNavigationItemContext, object?>? GetData { get; set; }

    [Parameter]
    public Func<AdaptiveNavigationMatchContext, bool>? Match { get; set; }

    [CascadingParameter]
    public NavHostLayoutContext? LayoutContext { get; set; }

    public INavigator Navigator => LayoutContext?.Nav ?? NullNavigator.Instance;

    Type? IAdaptiveNavigationHost.CurrentPageType => CurrentPageType ?? LayoutContext?.CurrentPageType;

    string? IAdaptiveNavigationHost.CurrentRoute => CurrentRoute ?? LayoutContext?.CurrentRoute;

    Func<AdaptiveNavigationItemContext, object?>? IAdaptiveNavigationHost.DefaultGetData => GetData;

    Func<AdaptiveNavigationMatchContext, bool>? IAdaptiveNavigationHost.DefaultMatch => Match;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Navigator.Navigated += OnNavigated;
    }

    private void OnNavigated(object? sender, Modals.NavigatedEventArgs e)
    {
        //force a re-render:
        InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<IAdaptiveNavigationHost>>(0);
        {
            builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
            builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
            builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), (RenderFragment)(builder2 =>
            {
                builder2.OpenElement(4, "nav");
                {
                    builder2.AddMultipleAttributes(5, GetAppliedAttributes());
                    builder2.AddContent(6, ChildContent);
                }
                builder2.CloseElement();
            }));
        }
        builder.CloseComponent();
    }

    protected override void BuildClass(ClassBuilder builder)
    {
        base.BuildClass(builder);
        builder.Add("au-adaptive-navigation");
    }

    public void Dispose()
    {
        Navigator.Navigated -= OnNavigated;
    }
}
