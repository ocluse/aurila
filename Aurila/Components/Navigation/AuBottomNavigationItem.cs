using Aurila.Components.Controls;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;

namespace Aurila.Components.Navigation;

public class AuBottomNavigationItem : AuClickableBase<AuBottomNavigationItem>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Overrides the selected state. When left unset it is derived from the current route.
    /// </summary>
    [Parameter]
    public bool? Selected { get; set; }

    [Parameter]
    public ActiveMatch? MatchMode { get; set; }

    protected override void BuildContent(RenderTreeBuilder builder)
    {
        builder.AddContent(0, ChildContent);
    }

    protected override void BuildControlClass(ClassBuilder builder)
    {
        base.BuildControlClass(builder);
        builder.Add("au-bottom-navigation-item")
            .AddIf(IsSelected(), "au-bottom-navigation-item--selected");
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

        if (Navigator is null || ResolvedUrl is null)
        {
            return false;
        }

        return RouteMatching.IsActive(Navigator.CurrentRoute, ResolvedUrl, MatchMode ?? ActiveMatch.Prefix);
    }
}
