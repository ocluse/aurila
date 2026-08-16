using Aurila.Components.Controls;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Components.Navigation;

public sealed class AuAdaptiveNavigationItem : AuClickableBase<AuAdaptiveNavigationItem>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool? Selected { get; set; }

    /// <summary>
    /// How the destination is compared with the current route. Falls back to the containing
    /// navigation's default.
    /// </summary>
    [Parameter]
    public ActiveMatch? MatchMode { get; set; }

    [CascadingParameter]
    private AuAdaptiveNavigation? Navigation { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-adaptive-navigation-item")
            .AddIf(IsSelected(), "au-adaptive-navigation-item--selected");
    }

    protected override void BuildAttributes(IDictionary<string, object> attributes)
    {
        base.BuildAttributes(attributes);

        if (IsSelected())
        {
            attributes["aria-current"] = "page";
        }
    }

    private bool IsSelected()
    {
        if (Selected.HasValue)
        {
            return Selected.Value;
        }

        if (Navigator is null || ResolvedUrl is not { } url)
        {
            return false;
        }

        var mode = MatchMode ?? Navigation?.DefaultMatchMode ?? ActiveMatch.Prefix;

        return RouteMatching.IsActive(Navigator.CurrentRoute, url, mode);
    }
}
