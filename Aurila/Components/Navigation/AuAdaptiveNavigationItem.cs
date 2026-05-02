using Aurila.Components.Controls;
using Aurila.Contracts.Navigation;
using Aurila.Design;
using Aurila.Enums.Navigation;
using Aurila.Models.Navigation;
using Aurila.Services.Navigation;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Components.Navigation;

public sealed class AuAdaptiveNavigationItem : AuClickableBase<AuAdaptiveNavigationItem>
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
    public Func<object?>? GetData { get; set; }

    [Parameter]
    public bool Replace { get; set; }

    [Parameter]
    public bool? Selected { get; set; }

    [Parameter]
    public ActiveMatch MatchMode { get; set; }

    [CascadingParameter]
    public IAurilaContext AurilaContext { get; set; } = null!;

    [CascadingParameter]
    INavigator Navigator { get; set; } = null!;

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

        if (Page != null)
        {
            var payload = ResolveData(e);

            if (Replace)
            {
                Navigator.Replace(Page, payload);
            }
            else
            {
                Navigator.Navigate(Page, payload);
            }
        }
        else if (Route.IsNotEmpty())
        {
            Navigator.Navigate(Route);
        }

        return Task.CompletedTask;
    }

    private object? ResolveData(MouseEventArgs e)
    {
        if (GetData != null)
        {
            return GetData();
        }
        else
        {
            return Data;
        }
    }

    private bool IsSelected()
    {
        if (Selected.HasValue)
        {
            return Selected.Value;
        }

        var currentPageType = Navigator.CurrentPageType;
        var currentRoute = AurilaContext.CurrentRoute.Value;

        var matchMode = MatchMode;
        

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
