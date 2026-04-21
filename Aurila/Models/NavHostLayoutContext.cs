using Aurila.Contracts.Navigation;

namespace Aurila.Models;

public record NavHostLayoutContext(
    INavigator Nav,
    Type? CurrentPageType,
    string? CurrentRoute,
    RenderFragment Content);
