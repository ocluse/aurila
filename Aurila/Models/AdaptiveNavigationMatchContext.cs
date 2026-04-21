namespace Aurila.Models;

public record AdaptiveNavigationMatchContext(
    Type? CurrentPageType,
    string? CurrentRoute,
    Type? ItemPage,
    string? ItemRoute,
    ActiveMatch MatchMode);
