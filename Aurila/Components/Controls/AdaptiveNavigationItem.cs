using Aurila.Components.Controls.Internal;
using Aurila.Contracts.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Controls;

public sealed class AdaptiveNavigationItem : ClickableBase<AdaptiveNavigationItem>
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public Type? Page { get; set; }

    [Parameter]
    public string? Route { get; set; }

    [Parameter]
    public object? Data { get; set; }

    [Parameter]
    public Func<AdaptiveNavigationItemContext, object?>? GetData { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool? Selected { get; set; }

    [Parameter]
    public ActiveMatch? MatchMode { get; set; }

    [Parameter]
    public Func<AdaptiveNavigationMatchContext, bool>? Match { get; set; }

    [CascadingParameter]
    public IAdaptiveNavigationHost? Host { get; set; }

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

    protected override Task OnClickedAsync(MouseEventArgs e)
    {
        if (Clicked.HasDelegate)
        {
            return Clicked.InvokeAsync(e);
        }

        var navigator = Host?.Navigator;
        if (navigator == null)
        {
            return Task.CompletedTask;
        }

        var payload = ResolveData(e);

        if (Page != null)
        {
            if (Replace)
            {
                navigator.Replace(Page, payload);
            }
            else
            {
                navigator.Navigate(Page, payload);
            }
        }else if (Route.IsNotEmpty())
        {
            navigator.Navigate(Route);
        }

        return Task.CompletedTask;
    }

    private object? ResolveData(MouseEventArgs e)
    {
        var context = new AdaptiveNavigationItemContext(
            Host?.Navigator ?? NullNavigator.Instance,
            Host?.CurrentPageType,
            Host?.CurrentRoute,
            e);

        var resolver = GetData ?? Host?.DefaultGetData;
        if (resolver != null)
        {
            return resolver(context);
        }

        return Data;
    }

    private bool IsSelected()
    {
        if (Selected.HasValue)
        {
            return Selected.Value;
        }

        var currentPageType = Host?.CurrentPageType;
        var currentRoute = Host?.CurrentRoute;
        var matchMode = MatchMode ?? Host?.DefaultMatchMode ?? ActiveMatch.Prefix;

        var context = new AdaptiveNavigationMatchContext(
            currentPageType,
            currentRoute,
            Page,
            Route,
            matchMode);

        var matcher = Match ?? Host?.DefaultMatch;
        if (matcher != null)
        {
            return matcher(context);
        }

        if (!string.IsNullOrWhiteSpace(Route) && !string.IsNullOrWhiteSpace(currentRoute))
        {
            if (matchMode == ActiveMatch.Exact)
            {
                return string.Equals(currentRoute, Route, StringComparison.OrdinalIgnoreCase);
            }

            if (currentRoute.Equals(Route, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var routePrefix = Route!.EndsWith('/') ? Route : $"{Route}/";
            return currentRoute.StartsWith(routePrefix, StringComparison.OrdinalIgnoreCase);
        }

        if (Page != null && currentPageType != null)
        {
            return Page == currentPageType;
        }

        return false;
    }
}
