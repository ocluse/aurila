using Aurila.Contracts.Navigation;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Models;

public record AdaptiveNavigationItemContext(
    INavigator Navigator,
    Type? CurrentPageType,
    string? CurrentRoute,
    MouseEventArgs MouseEventArgs);
