using Aurila.Contracts.Navigation;
using Microsoft.AspNetCore.Components.Web;

namespace Aurila.Models.Navigation;

public record AdaptiveNavigationItemContext(
    INavigator Navigator,
    Type? CurrentPageType,
    string CurrentRoute,
    MouseEventArgs MouseEventArgs);
