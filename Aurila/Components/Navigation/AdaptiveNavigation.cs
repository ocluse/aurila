using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;

namespace Aurila.Components.Navigation;

public sealed class AdaptiveNavigation : ControlBase<AdaptiveNavigation>, IDisposable
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public ActiveMatch DefaultMatchMode { get; set; } = ActiveMatch.Prefix;

    [Parameter]
    public Func<AdaptiveNavigationItemContext, object?>? GetData { get; set; }

    [CascadingParameter]
    public NavHostLayoutContext? LayoutContext { get; set; }

    [CascadingParameter]
    INavigator Navigator { get; set; } = null!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Navigator.Navigated += OnNavigated;
    }

    private void OnNavigated(object? sender, NavigatedEventArgs e)
    {
        //force a re-render:
        InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<AdaptiveNavigation>>(0);
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
